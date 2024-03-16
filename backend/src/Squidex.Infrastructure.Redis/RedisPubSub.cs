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
        private readonly IConnectionMultiplexer redis;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private ISubscriber redisSubscriber;

        public RedisPubSub(IConnectionMultiplexer redis, IJsonSerializer serializer, ISemanticLog log)
        {
            this.log = log;
            this.redis = redis;
            this.serializer = serializer;
        }

        public Task InitializeAsync(
            CancellationToken ct = default)
        {
            try
            {
                redisSubscriber = redis.GetSubscriber();

                redis.GetStatus();

                return TaskHelper.Done;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redis.Configuration}", ex);
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

            return (RedisSubscription<T>)subscriptions.GetOrAdd(typeName, this, (k, c) => new RedisSubscription<T>(c.redisSubscriber, serializer, k, c.log));
        }
    }
}
