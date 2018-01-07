// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using Squidex.Infrastructure.Log;
using StackExchange.Redis;

namespace Squidex.Infrastructure
{
    public sealed class RedisPubSub : IPubSub, IInitializable
    {
        private readonly ConcurrentDictionary<string, object> subscriptions = new ConcurrentDictionary<string, object>();
        private readonly Lazy<IConnectionMultiplexer> redisClient;
        private readonly Lazy<ISubscriber> redisSubscriber;
        private readonly ISemanticLog log;

        public RedisPubSub(Lazy<IConnectionMultiplexer> redis, ISemanticLog log)
        {
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(log, nameof(log));

            this.log = log;

            redisClient = redis;
            redisSubscriber = new Lazy<ISubscriber>(() => redis.Value.GetSubscriber());
        }

        public void Initialize()
        {
            try
            {
                redisClient.Value.GetStatus();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redisClient.Value.Configuration}", ex);
            }
        }

        public void Publish<T>(T value, bool notifySelf)
        {
            GetSubscriber<T>().Publish(value, notifySelf);
        }

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            return GetSubscriber<T>().Subscribe(handler);
        }

        private RedisSubscription<T> GetSubscriber<T>()
        {
            var typeName = typeof(T).FullName;

            return (RedisSubscription<T>)subscriptions.GetOrAdd(typeName, c => new RedisSubscription<T>(redisSubscriber.Value, c, log));
        }
    }
}
