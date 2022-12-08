// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;
using Squidex.Infrastructure.Timers;
using Squidex.Messaging;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public sealed class EventConsumerWorker :
    IMessageHandler<EventConsumerStart>,
    IMessageHandler<EventConsumerStop>,
    IMessageHandler<EventConsumerReset>,
    IBackgroundProcess
{
    private readonly Dictionary<string, EventConsumerProcessor> processors = new Dictionary<string, EventConsumerProcessor>();
    private CompletionTimer? timer;

    public EventConsumerWorker(IEnumerable<IEventConsumer> eventConsumers,
        Func<IEventConsumer, EventConsumerProcessor> factory)
    {
        foreach (var consumer in eventConsumers)
        {
            processors[consumer.Name] = factory(consumer);
        }
    }

    public async Task StartAsync(
        CancellationToken ct)
    {
        foreach (var (_, processor) in processors)
        {
            await processor.InitializeAsync(ct);
            await processor.ActivateAsync();
        }

        timer = new CompletionTimer(TimeSpan.FromSeconds(30), async ct =>
        {
            foreach (var (_, processor) in processors)
            {
                await processor.ActivateAsync();
            }
        });
    }

    public Task StopAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task HandleAsync(EventConsumerStart message,
        CancellationToken ct)
    {
        if (processors.TryGetValue(message.Name, out var processor))
        {
            await processor.StartAsync();
        }
    }

    public async Task HandleAsync(EventConsumerStop message,
        CancellationToken ct)
    {
        if (processors.TryGetValue(message.Name, out var processor))
        {
            await processor.StopAsync();
        }
    }

    public async Task HandleAsync(EventConsumerReset message,
        CancellationToken ct)
    {
        if (processors.TryGetValue(message.Name, out var processor))
        {
            await processor.ResetAsync();
        }
    }
}
