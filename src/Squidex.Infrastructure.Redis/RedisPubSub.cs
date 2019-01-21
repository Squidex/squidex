// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using StackExchange.Redis;

namespace Squidex.Infrastructure
{
    public sealed class RedisPubSub : IPubSub, IInitializable
    {
        private readonly ConcurrentDictionary<string, object> subscriptions = new ConcurrentDictionary<string, object>();
        private readonly Lazy<IConnectionMultiplexer> redisClient;
        private readonly IJsonSerializer serializer;
        private readonly Lazy<ISubscriber> redisSubscriber;
        private readonly ISemanticLog log;

        public RedisPubSub(Lazy<IConnectionMultiplexer> redis, IJsonSerializer serializer, ISemanticLog log)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(log, nameof(log));

            this.log = log;

            redisClient = redis;
            redisSubscriber = new Lazy<ISubscriber>(() => redis.Value.GetSubscriber());

            this.serializer = serializer;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                redisClient.Value.GetStatus();

                return TaskHelper.Done;
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

            return (RedisSubscription<T>)subscriptions.GetOrAdd(typeName, this, (k, c) => new RedisSubscription<T>(c.redisSubscriber.Value, serializer, k, c.log));
        }
    }
}
