// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;
using Squidex.Messaging;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerWorker :
        IMessageHandler<EventConsumerStart>,
        IMessageHandler<EventConsumerStop>,
        IMessageHandler<EventConsumerReset>,
        IInitializable
    {
        private readonly Dictionary<string, EventConsumerProcessor> processors = new Dictionary<string, EventConsumerProcessor>();

        public EventConsumerWorker(IEnumerable<IEventConsumer> eventConsumers, IServiceProvider serviceProvider)
        {
            foreach (var consumer in eventConsumers)
            {
                var processor = ActivatorUtilities.CreateInstance<EventConsumerProcessor>(serviceProvider, consumer);

                processors[consumer.Name] = processor;
            }
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            foreach (var (_, processor) in processors)
            {
                await processor.InitializeAsync(ct);
            }
        }

        public async Task HandleAsync(EventConsumerStart message,
            CancellationToken ct = default)
        {
            if (processors.TryGetValue(message.EventConsumer, out var processor))
            {
                await processor.StartAsync();
            }
        }

        public async Task HandleAsync(EventConsumerStop message,
            CancellationToken ct = default)
        {
            if (processors.TryGetValue(message.EventConsumer, out var processor))
            {
                await processor.StopAsync();
            }
        }

        public async Task HandleAsync(EventConsumerReset message,
            CancellationToken ct = default)
        {
            if (processors.TryGetValue(message.EventConsumer, out var processor))
            {
                await processor.ResetAsync();
            }
        }
    }
}
