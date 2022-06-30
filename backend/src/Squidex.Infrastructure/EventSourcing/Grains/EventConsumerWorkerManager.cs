// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerWorkerManager :
        IInitializable,
        IConsumer<EventConsumerStart>,
        IConsumer<EventConsumerStop>,
        IConsumer<EventConsumerReset>
    {
        private readonly Dictionary<string, EventConsumerWorker> workers = new Dictionary<string, EventConsumerWorker>();

        public EventConsumerWorkerManager(IEnumerable<IEventConsumer> eventConsumers, IServiceProvider serviceProvider)
        {
            foreach (var consumer in eventConsumers)
            {
                var worker = ActivatorUtilities.CreateInstance<EventConsumerWorker>(serviceProvider, consumer);

                workers[consumer.Name] = worker;
            }
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            foreach (var (_, worker) in workers)
            {
                await worker.InitializeAsync(ct);
            }
        }

        public async Task Consume(ConsumeContext<EventConsumerStart> context)
        {
            if (workers.TryGetValue(context.Message.EventConsumer, out var worker))
            {
                await worker.StartAsync();
            }
        }

        public async Task Consume(ConsumeContext<EventConsumerStop> context)
        {
            if (workers.TryGetValue(context.Message.EventConsumer, out var worker))
            {
                await worker.StopAsync();
            }
        }

        public async Task Consume(ConsumeContext<EventConsumerReset> context)
        {
            if (workers.TryGetValue(context.Message.EventConsumer, out var worker))
            {
                await worker.ResetAsync();
            }
        }
    }
}
