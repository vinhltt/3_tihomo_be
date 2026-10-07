using System.Collections.Concurrent;
using MassTransit;

namespace Tests.Shared.Helpers;

/// <summary>
///     Subscribes to real broker messages on a private temporary queue (EN)<br/>
///     Lắng nghe message thật trên broker qua queue tạm riêng (VI)
/// </summary>
/// <typeparam name="TMessage">Production message contract to observe (never a test copy)</typeparam>
public sealed class RabbitMqResultObserver<TMessage> : IAsyncDisposable where TMessage : class
{
    private readonly IBusControl _bus;
    private readonly ConcurrentQueue<TMessage> _received = new();

    private RabbitMqResultObserver(IBusControl bus) => _bus = bus;

    /// <summary>Starts the bus and binds a temporary, auto-deleted queue before any message is produced.</summary>
    public static async Task<RabbitMqResultObserver<TMessage>> StartAsync()
    {
        var uri = new Uri(ExternalBackend.Require("TIHOMO_TEST_RABBITMQ"));
        var user = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]);
        var pass = Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[1]);
        RabbitMqResultObserver<TMessage>? observer = null;

        var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(uri.Host, (ushort)uri.Port, "/", h =>
            {
                h.Username(user);
                h.Password(pass);
            });
            cfg.ReceiveEndpoint($"characterization-{typeof(TMessage).Name}-{Guid.NewGuid():N}", e =>
            {
                // RabbitMQ 4 rejects transient non-exclusive queues: exclusive + auto-delete is the private temp queue
                e.Exclusive = true;
                e.AutoDelete = true;
                e.Durable = false;
                e.Handler<TMessage>(ctx =>
                {
                    observer!._received.Enqueue(ctx.Message);
                    return Task.CompletedTask;
                });
            });
        });
        observer = new RabbitMqResultObserver<TMessage>(bus);
        await bus.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
        return observer;
    }

    /// <summary>Waits a bounded time for the first matching message; fails with a diagnostic on timeout.</summary>
    public async Task<TMessage> WaitForAsync(Func<TMessage, bool> match, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var hit = _received.FirstOrDefault(match);
            if (hit is not null) return hit;
            await Task.Delay(100);
        }

        throw new TimeoutException(
            $"No {typeof(TMessage).Name} matched within {timeout.TotalSeconds}s (received {_received.Count} unmatched).");
    }

    public async ValueTask DisposeAsync() => await _bus.StopAsync();
}
