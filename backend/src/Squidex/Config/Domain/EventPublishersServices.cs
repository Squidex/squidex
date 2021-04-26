// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting.Configuration;
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
                    var error = new ConfigurationError("Value is required.", "eventPublishers:{child.Key}:type");

                    throw new ConfigurationException(error);
                }

                var eventsFilter = child.GetValue<string>("eventsFilter");

                var enabled = child.GetValue<bool>("enabled");

                if (string.Equals(eventPublisherType, "RabbitMq", StringComparison.OrdinalIgnoreCase))
                {
                    var publisherConfig = child.GetValue<string>("configuration");

                    if (string.IsNullOrWhiteSpace(publisherConfig))
                    {
                        var error = new ConfigurationError("Value is required.", "eventPublishers:{child.Key}:configuration");

                        throw new ConfigurationException(error);
                    }

                    var exchange = child.GetValue<string>("exchange");

                    if (string.IsNullOrWhiteSpace(exchange))
                    {
                        var error = new ConfigurationError("Value is required.", "eventPublishers:{child.Key}:exchange");

                        throw new ConfigurationException(error);
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
                    var error = new ConfigurationError($"Unsupported value '{child.Key}", "eventPublishers:{child.Key}:type.");

                    throw new ConfigurationException(error);
                }
            }
        }
    }
}
