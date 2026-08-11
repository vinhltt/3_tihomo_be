using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Shared.Contracts;

namespace MessageQueueTester;

/// <summary>
/// Simple console application to test message queue functionality
/// Ứng dụng console đơn giản để test chức năng message queue
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/message-test-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.WithProperty("Application", "MessageQueueTester")
            .CreateLogger();

        try
        {
            Console.WriteLine("🚀 Starting Message Queue Test...");
            
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Test message publishing
            var publisher = serviceProvider.GetRequiredService<IPublishEndpoint>();
            var correlationService = serviceProvider.GetRequiredService<ICorrelationContextService>();
            
            // Set correlation context
            correlationService.SetCorrelationId(Guid.CreateVersion7());
            
            var testMessage = new UploadTransactionDataMessage
            {
                CorrelationId = correlationService.CorrelationId,
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
            
            // Keep the application running for a bit to see if consumer picks up the message
            Console.WriteLine("⏳ Waiting for message processing...");
            await Task.Delay(5000);
            
            Console.WriteLine("✅ Message queue test completed!");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Message queue test failed");
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        finally
        {
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
        
        // Add correlation context service
        services.AddSingleton<ICorrelationContextService, CorrelationContextService>();
        
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
