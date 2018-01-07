// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using StackExchange.Redis;

namespace Squidex.Config.Domain
{
    public static class PubSubServices
    {
        public static void AddMyPubSubServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("pubSub:type", new Options
            {
                ["InMemory"] = () =>
                {
                    services.AddSingletonAs<InMemoryPubSub>()
                        .As<IPubSub>();
                },
                ["Redis"] = () =>
                {
                    var configuration = config.GetRequiredValue("pubsub:redis:configuration");

                    var redis = Singletons<IConnectionMultiplexer>.GetOrAddLazy(configuration, s => ConnectionMultiplexer.Connect(s));

                    services.AddSingletonAs(c => new RedisPubSub(redis, c.GetRequiredService<ISemanticLog>()))
                        .As<IPubSub>()
                        .As<IInitializable>();
                }
            });
        }
    }
}
