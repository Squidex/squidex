// ==========================================================================
//  ClusterModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

            if (string.Equals(clustererType, "Slack", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = Configuration.GetValue<string>("squidex:clusterer:redis:connectionString");

                if (string.IsNullOrWhiteSpace(connectionString) || !Uri.IsWellFormedUriString(connectionString, UriKind.Absolute))
                {
                    throw new ConfigurationException("You must specify the Redis connection string in the 'squidex:clusterer:redis:connectionString' configuration section.");
                }
                
                builder.Register(c => ConnectionMultiplexer.Connect(connectionString))
                    .As<IConnectionMultiplexer>()
                    .SingleInstance();

                builder.RegisterType<RedisEventNotifier>()
                    .As<IEventNotifier>()
                    .SingleInstance();

                builder.RegisterType<RedisExternalSystem>()
                    .As<IExternalSystem>()
                    .SingleInstance();

                builder.Register(c =>
                    {
                        var inner = new MemoryCache(c.Resolve<IOptions<MemoryCacheOptions>>());

                        return new RedisInvalidatingCache(inner,
                            c.Resolve<IConnectionMultiplexer>(),
                            c.Resolve<ILogger<RedisInvalidatingCache>>());
                    })
                    .As<IMemoryCache>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported clusterer type '{clustererType}' for key 'squidex:clusterer:type', supported: Redis.");
            }
        }
    }
}
