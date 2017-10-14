// ==========================================================================
//  EventStoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Autofac.Core;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors;

namespace Squidex.Config.Domain
{
    public sealed class EventStoreModule : Module
    {
        private const string MongoClientRegistration = "EventStoreMongoClient";
        private const string MongoDatabaseRegistration = "EventStoreMongoDatabase";

        private IConfiguration Configuration { get; }

        public EventStoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var consumeEvents = Configuration.GetValue<bool>("eventStore:consume");

            if (consumeEvents)
            {
                builder.RegisterType<EventConsumerActor>()
                    .AsSelf()
                    .InstancePerDependency();
            }

            var eventStoreType = Configuration.GetValue<string>("eventStore:type");

            if (string.IsNullOrWhiteSpace(eventStoreType))
            {
                throw new ConfigurationException("Configure EventStore type with 'eventStore:type'.");
            }

            if (string.Equals(eventStoreType, "MongoDb", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = Configuration.GetValue<string>("eventStore:mongoDb:configuration");

                if (string.IsNullOrWhiteSpace(configuration))
                {
                    throw new ConfigurationException("Configure EventStore MongoDb configuration with 'eventStore:mongoDb:configuration'.");
                }

                var database = Configuration.GetValue<string>("eventStore:mongoDb:database");

                if (string.IsNullOrWhiteSpace(database))
                {
                    throw new ConfigurationException("Configure EventStore MongoDb Database name with 'eventStore:mongoDb:database'.");
                }

                builder.Register(c => Singletons<IMongoClient>.GetOrAdd(configuration, s => new MongoClient(s)))
                    .Named<IMongoClient>(MongoClientRegistration)
                    .SingleInstance();

                builder.Register(c => c.ResolveNamed<IMongoClient>(MongoClientRegistration).GetDatabase(database))
                    .Named<IMongoDatabase>(MongoDatabaseRegistration)
                    .SingleInstance();

                builder.RegisterType<MongoEventStore>()
                    .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else if (string.Equals(eventStoreType, "GetEventStore", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = Configuration.GetValue<string>("eventStore:getEventStore:configuration");

                if (string.IsNullOrWhiteSpace(configuration))
                {
                    throw new ConfigurationException("Configure GetEventStore EventStore configuration with 'eventStore:getEventStore:configuration'.");
                }

                var projectionHost = Configuration.GetValue<string>("eventStore:getEventStore:projectionHost");

                if (string.IsNullOrWhiteSpace(projectionHost))
                {
                    throw new ConfigurationException("Configure GetEventStore EventStore projection host with 'eventStore:getEventStore:projectionHost'.");
                }

                var prefix = Configuration.GetValue<string>("eventStore:getEventStore:prefix");

                var connection = EventStoreConnection.Create(configuration);

                builder.Register(c => new GetEventStore(connection, prefix, projectionHost))
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{eventStoreType}' for 'eventStore:type', supported: MongoDb, GetEventStore.");
            }
        }
    }
}
