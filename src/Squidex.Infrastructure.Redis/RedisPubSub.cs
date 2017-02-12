// ==========================================================================
//  RedisInvalidator2.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis
{
    public class RedisPubSub : IPubSub, IExternalSystem
    {
        private readonly ConcurrentDictionary<string, RedisSubscription> subscriptions = new ConcurrentDictionary<string, RedisSubscription>();
        private readonly IConnectionMultiplexer redis;
        private readonly ILogger<RedisPubSub> logger;
        private readonly ISubscriber subscriber;

        public RedisPubSub(IConnectionMultiplexer redis, ILogger<RedisPubSub> logger)
        {
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(logger, nameof(logger));

            this.redis = redis;

            this.logger = logger;

            subscriber = redis.GetSubscriber();
        }

        public void Connect()
        {
            try
            {
                redis.GetStatus();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redis.Configuration}", ex);
            }
        }

        public void Publish(string channelName, string token, bool notifySelf)
        {
            Guard.NotNullOrEmpty(channelName, nameof(channelName));

            subscriptions.GetOrAdd(channelName, c => new RedisSubscription(subscriber, c, logger)).Publish(token, notifySelf);
        }

        public IDisposable Subscribe(string channelName, Action<string> handler)
        {
            Guard.NotNullOrEmpty(channelName, nameof(channelName));

            return subscriptions.GetOrAdd(channelName, c => new RedisSubscription(subscriber, c, logger)).Subscribe(handler);
        }
    }
}
