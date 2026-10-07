using CoreFinance.Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MessageQueueTester;

/// <summary>
/// Simple console application to test in-memory message publishing (no broker, no consumer)<br/>
/// Ứng dụng console đơn giản để test publish message qua in-memory bus (không broker, không consumer)
/// </summary>
class Program
{
    /// <returns>0 when the message was published and the bus stopped cleanly; 1 otherwise</returns>
    static async Task<int> Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/message-test-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.WithProperty("Application", "MessageQueueTester")
            .CreateLogger();

        ServiceProvider? serviceProvider = null;
        IBusControl? bus = null;
        try
        {
            Console.WriteLine("🚀 Starting Message Queue Test...");

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();

            // ServiceCollection (no Generic Host) does not start the bus: own the lifecycle here
            bus = serviceProvider.GetRequiredService<IBusControl>();
            await bus.StartAsync();

            using var scope = serviceProvider.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var testMessage = new UploadTransactionDataMessage
            {
                CorrelationId = Guid.CreateVersion7(),
                FileName = "test-file.xlsx",
                UploadedAt = DateTime.UtcNow,
                TransactionData = new List<TransactionDataRow>
                {
                    new TransactionDataRow
                    {
                        TransactionDate = DateTime.Today,
                        Description = "Test Transaction 1",
                        Amount = 100.50m,
                        Reference = "REF001"
                    },
                    new TransactionDataRow
                    {
                        TransactionDate = DateTime.Today.AddDays(-1),
                        Description = "Test Transaction 2",
                        Amount = -50.25m,
                        Reference = "REF002"
                    }
                }
            };

            Log.Information("📤 Publishing test message with CorrelationId: {CorrelationId}",
                testMessage.CorrelationId);

            await publisher.Publish(testMessage);
            Console.WriteLine("✅ Message published successfully!");
            Console.WriteLine($"📋 CorrelationId: {testMessage.CorrelationId}");
            Console.WriteLine($"📊 Transaction count: {testMessage.TransactionData.Count}");

            await bus.StopAsync();
            bus = null;

            Console.WriteLine("✅ Message queue test completed!");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Message queue test failed");
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
        finally
        {
            if (bus is not null)
            {
                try { await bus.StopAsync(); } catch { /* bus failed to start or already stopped */ }
            }
            if (serviceProvider is not null) await serviceProvider.DisposeAsync();
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Configure MassTransit with in-memory transport for testing
        services.AddMassTransit(x =>
        {
            // Use in-memory transport for testing without RabbitMQ
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
