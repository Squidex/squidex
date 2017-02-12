// ==========================================================================
//  ClusterModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Redis;
using StackExchange.Redis;

namespace Squidex.Config.Domain
{
    public class ClusterModule : Module
    {
        public IConfiguration Configuration { get; }

        public ClusterModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var handleEvents = Configuration.GetValue<bool>("squidex:handleEvents");

            if (handleEvents)
            {
                builder.RegisterType<EventReceiver>()
                    .AsSelf()
                    .InstancePerDependency();
            }

            var clustererType = Configuration.GetValue<string>("squidex:clusterer:type");

            if (string.IsNullOrWhiteSpace(clustererType))
            {
                throw new ConfigurationException("You must specify the clusterer type in the 'squidex:clusterer:type' configuration section.");
            }

            if (string.Equals(clustererType, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = Configuration.GetValue<string>("squidex:clusterer:redis:connectionString");

                if (string.IsNullOrWhiteSpace(connectionString) || !Uri.IsWellFormedUriString(connectionString, UriKind.Absolute))
                {
                    throw new ConfigurationException("You must specify the Redis connection string in the 'squidex:clusterer:redis:connectionString' configuration section.");
                }

                try
                {
                    var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);

                    builder.RegisterInstance(connectionMultiplexer)
                        .As<IConnectionMultiplexer>()
                        .SingleInstance();
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException($"Redis connection failed to connect to database {connectionString}", ex);
                }

                builder.RegisterType<RedisPubSub>()
                    .As<IPubSub>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else if (string.Equals(clustererType, "None", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterType<InMemoryPubSub>()
                    .As<IPubSub>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported clusterer type '{clustererType}' for key 'squidex:clusterer:type', supported: Redis, None.");
            }
        }
    }
}
