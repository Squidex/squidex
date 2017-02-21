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
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb.EventStore;

namespace Squidex.Config.Domain
{
    public class EventStoreModule : Module
    {
        public IConfiguration Configuration { get; }

        public EventStoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var storeType = Configuration.GetValue<string>("squidex:eventStore:type");

            if (string.IsNullOrWhiteSpace(storeType))
            {
                throw new ConfigurationException("You must specify the store type in the 'squidex:eventStore:type' configuration section.");
            }

            if (string.Equals(storeType, "MongoDb", StringComparison.OrdinalIgnoreCase))
            {
                var databaseName = Configuration.GetValue<string>("squidex:eventStore:mongoDb:databaseName");

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new ConfigurationException("You must specify the MongoDB database name in the 'squidex:eventStore:mongoDb:databaseName' configuration section.");
                }

                var connectionString = Configuration.GetValue<string>("squidex:eventStore:mongoDb:connectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationException("You must specify the MongoDB connection string in the 'squidex:eventStore:mongoDb:connectionString' configuration section.");
                }

                builder.Register(c =>
                    {
                        var mongoDbClient = new MongoClient(connectionString);
                        var mongoDatabase = mongoDbClient.GetDatabase(databaseName);

                        var eventStore = new MongoEventStore(mongoDatabase, c.Resolve<IEventNotifier>(), c.Resolve<IClock>());

                        return eventStore;
                    })
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported store type '{storeType}' for key 'squidex:eventStore:type', supported: MongoDb.");
            }
        }
    }
}
