// ==========================================================================
//  PubSubModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Redis;
using StackExchange.Redis;

namespace Squidex.Config.Domain
{
    public sealed class PubSubModule : Module
    {
        private const string RedisRegistration = "PubSubRedis";

        private IConfiguration Configuration { get; }

        public PubSubModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var pubSubType = Configuration.GetValue<string>("pubSub:type");

            if (string.IsNullOrWhiteSpace(pubSubType))
            {
                throw new ConfigurationException("Configure the PubSub type with 'pubSub:type'.");
            }

            if (string.Equals(pubSubType, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = Configuration.GetValue<string>("pubsub:redis:configuration");

                if (string.IsNullOrWhiteSpace(configuration))
                {
                    throw new ConfigurationException("Configure PubSub Redis configuration with 'pubSub:redis:configuration'.");
                }

                builder.Register(c => Singletons<IConnectionMultiplexer>.GetOrAdd(configuration, s => ConnectionMultiplexer.Connect(s)))
                    .Named<IConnectionMultiplexer>(RedisRegistration)
                    .SingleInstance();

                builder.RegisterType<RedisPubSub>()
                    .WithParameter(ResolvedParameter.ForNamed<IConnectionMultiplexer>(RedisRegistration))
                    .As<IPubSub>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else if (string.Equals(pubSubType, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterType<InMemoryPubSub>()
                    .As<IPubSub>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{pubSubType}' for 'pubSub:type', supported: Redis, InMemory.");
            }
        }
    }
}
