// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Config.Domain
{
    public static class EventPublishersServices
    {
        public static void AddSquidexEventPublisher(this IServiceCollection services, IConfiguration config)
        {
            var eventPublishers = config.GetSection("eventPublishers");

            foreach (var child in eventPublishers.GetChildren())
            {
                var eventPublisherType = child.GetValue<string>("type");

                if (string.IsNullOrWhiteSpace(eventPublisherType))
                {
                    throw new ConfigurationException($"Configure EventPublisher type with 'eventPublishers:{child.Key}:type'.");
                }

                var eventsFilter = child.GetValue<string>("eventsFilter");

                var enabled = child.GetValue<bool>("enabled");

                if (string.Equals(eventPublisherType, "RabbitMq", StringComparison.OrdinalIgnoreCase))
                {
                    var publisherConfig = child.GetValue<string>("configuration");

                    if (string.IsNullOrWhiteSpace(publisherConfig))
                    {
                        throw new ConfigurationException($"Configure EventPublisher RabbitMq configuration with 'eventPublishers:{child.Key}:configuration'.");
                    }

                    var exchange = child.GetValue<string>("exchange");

                    if (string.IsNullOrWhiteSpace(exchange))
                    {
                        throw new ConfigurationException($"Configure EventPublisher RabbitMq exchange with 'eventPublishers:{child.Key}:configuration'.");
                    }

                    var name = $"EventPublishers_{child.Key}";

                    if (enabled)
                    {
                        services.AddSingletonAs(c => new RabbitMqEventConsumer(c.GetRequiredService<IJsonSerializer>(), name, publisherConfig, exchange, eventsFilter))
                            .As<IEventConsumer>();
                    }
                }
                else
                {
                    throw new ConfigurationException($"Unsupported value '{child.Key}' for 'eventPublishers:{child.Key}:type', supported: RabbitMq.");
                }
            }
        }
    }
}
