// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class DefaultEventConsumerFactory : IEventConsumerFactory
    {
        private readonly Dictionary<string, IEventConsumer> eventConsumers;

        public DefaultEventConsumerFactory(IEnumerable<IEventConsumer> eventConsumers)
        {
            this.eventConsumers = eventConsumers.ToDictionary(x => x.Name);
        }

        public IEventConsumer Create(string name)
        {
            Guard.NotNullOrEmpty(name);

            if (!eventConsumers.TryGetValue(name, out var eventConsumer))
            {
                throw new ArgumentException($"Cannot find event consuemr with name '{name}'", nameof(name));
            }

            return eventConsumer;
        }
    }
}
