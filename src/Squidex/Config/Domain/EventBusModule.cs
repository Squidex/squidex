// ==========================================================================
//  EventStoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.RabbitMq;

namespace Squidex.Config.Domain
{
    public class EventBusModule : Module
    {
        public IConfiguration Configuration { get; }

        public EventBusModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var eventBusType = Configuration.GetValue<string>("squidex:eventBus:type");

            if (string.IsNullOrWhiteSpace(eventBusType))
            {
                throw new ConfigurationException("You must specify the event bus type in the 'squidex:eventBus:type' configuration section.");
            }

            var canCatch = Configuration.GetValue<bool>("squidex:eventBus:catch");

            builder.RegisterType<EventReceiver>()
                .WithParameter(new NamedParameter("canCatch", canCatch))
                .AsSelf()
                .SingleInstance();

            if (string.Equals(eventBusType, "Memory", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterType<InMemoryEventBus>()
                    .As<IEventStream>()
                    .As<IEventPublisher>()
                    .SingleInstance();
            }
            else if (string.Equals(eventBusType, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = Configuration.GetValue<string>("squidex:eventBus:rabbitMq:connectionString");

                if (string.IsNullOrWhiteSpace(connectionString) || !Uri.IsWellFormedUriString(connectionString, UriKind.Absolute))
                {
                    throw new ConfigurationException("You must specify the RabbitMq connection string in the 'squidex:eventBus:rabbitMq:connectionString' configuration section.");
                }

                var queueName = Configuration.GetValue<string>("squidex:eventBus:rabbitMq:queueName");

                builder.Register(c =>
                    {
                        var connectionFactory = new ConnectionFactory();

                        connectionFactory.SetUri(new Uri(connectionString));

                        return new RabbitMqEventBus(connectionFactory, canCatch, queueName);
                    })
                    .As<IEventStream>()
                    .As<IEventPublisher>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported store type '{eventBusType}' for key 'squidex:eventStore:type', supported: Memory, RabbmitMq.");
            }
        }
    }
}
