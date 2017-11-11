// ==========================================================================
//  EventPublishersServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Config.Domain
{
    public static class EventPublishersServices
    {
        public static void AddMyEventPublishersServices(this IServiceCollection services, IConfiguration configuration)
        {
            var eventPublishers = configuration.GetSection("eventPublishers");

            foreach (var child in eventPublishers.GetChildren())
            {
                var eventPublisherType = child.GetValue<string>("type");

                if (string.IsNullOrWhiteSpace(eventPublisherType))
                {
                    throw new ConfigurationException($"Configure EventPublisher type with 'eventPublishers:{child.Key}:type'.");
                }

                var eventsFilter = configuration.GetValue<string>("eventsFilter");

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
                        services.AddSingleton(c => new RabbitMqEventConsumer(c.GetRequiredService<JsonSerializerSettings>(), name, publisherConfig, exchange, eventsFilter))
                            .As<IEventConsumer>()
                            .As<IExternalSystem>();
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
