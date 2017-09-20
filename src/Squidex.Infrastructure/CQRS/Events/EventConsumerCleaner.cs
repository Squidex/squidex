// ==========================================================================
//  EventConsumerCleaner.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventConsumerCleaner
    {
        private readonly IEnumerable<IEventConsumer> eventConsumers;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;

        public EventConsumerCleaner(IEnumerable<IEventConsumer> eventConsumers, IEventConsumerInfoRepository eventConsumerInfoRepository)
        {
            Guard.NotNull(eventConsumers, nameof(eventConsumers));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.eventConsumers = eventConsumers;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
        }

        public Task CleanAsync()
        {
            var names = eventConsumers.Select(x => x.Name).ToArray();

            return eventConsumerInfoRepository.ClearAsync(names);
        }
    }
}