// ==========================================================================
//  RedisInvalidator2.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using Squidex.Infrastructure.Log;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis
{
    public class RedisPubSub : IPubSub, IExternalSystem
    {
        private readonly ConcurrentDictionary<string, RedisSubscription> subscriptions = new ConcurrentDictionary<string, RedisSubscription>();
        private readonly IConnectionMultiplexer redisClient;
        private readonly ISemanticLog log;
        private readonly ISubscriber redisSubscriber;

        public RedisPubSub(IConnectionMultiplexer redis, ISemanticLog log)
        {
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(log, nameof(log));

            this.log = log;

            redisClient = redis;
            redisSubscriber = redis.GetSubscriber();
        }

        public void Connect()
        {
            try
            {
                redisClient.GetStatus();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redisClient.Configuration}", ex);
            }
        }

        public void Publish(string channelName, string token, bool notifySelf)
        {
            Guard.NotNullOrEmpty(channelName, nameof(channelName));

            subscriptions.GetOrAdd(channelName, c => new RedisSubscription(redisSubscriber, c, log)).Publish(token, notifySelf);
        }

        public IDisposable Subscribe(string channelName, Action<string> handler)
        {
            Guard.NotNullOrEmpty(channelName, nameof(channelName));

            return subscriptions.GetOrAdd(channelName, c => new RedisSubscription(redisSubscriber, c, log)).Subscribe(handler);
        }
    }
}
