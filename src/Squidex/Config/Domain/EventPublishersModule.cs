// ==========================================================================
//  EventPublishersModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Config.Domain
{
    public sealed class EventPublishersModule : Module
    {
        private IConfiguration Configuration { get; }

        public EventPublishersModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var eventPublishers = Configuration.GetSection("eventPublishers");

            foreach (var child in eventPublishers.GetChildren())
            {
                var eventPublisherType = child.GetValue<string>("type");

                if (string.IsNullOrWhiteSpace(eventPublisherType))
                {
                    throw new ConfigurationException($"Configure EventPublisher type with 'eventPublishers:{child.Key}:type'.");
                }

                var eventsFilter = Configuration.GetValue<string>("eventsFilter");

                var enabled = child.GetValue<bool>("enabled");

                if (string.Equals(eventPublisherType, "RabbitMq", StringComparison.OrdinalIgnoreCase))
                {
                    var configuration = child.GetValue<string>("configuration");

                    if (string.IsNullOrWhiteSpace(configuration))
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
                        builder.Register(c => new RabbitMqEventConsumer(c.Resolve<JsonSerializerSettings>(), name, configuration, exchange, eventsFilter))
                            .As<IEventConsumer>()
                            .As<IExternalSystem>()
                            .SingleInstance();
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
