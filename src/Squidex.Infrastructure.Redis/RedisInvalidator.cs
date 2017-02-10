// ==========================================================================
//  RedisInvalidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

// ReSharper disable InvertIf
// ReSharper disable ArrangeThisQualifier

namespace Squidex.Infrastructure.Redis
{
    internal sealed class RedisInvalidator
    {
        private const string Channel = "SquidexChannelInvalidations";
        private readonly Guid instanceId = Guid.NewGuid();
        private readonly ISubscriber subscriber;
        private readonly IMemoryCache cache;
        private readonly ILogger<RedisInvalidatingCache> logger;
        private int invalidationsReceived;

        public int InvalidationsReceived
        {
            get
            {
                return invalidationsReceived;
            }
        }

        public RedisInvalidator(IConnectionMultiplexer redis, IMemoryCache cache, ILogger<RedisInvalidatingCache> logger)
        {
            this.cache = cache;

            subscriber = redis.GetSubscriber();
            subscriber.Subscribe(Channel, (channel, value) => HandleInvalidation(value));

            this.logger = logger;
        }

        private void HandleInvalidation(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var parts = value.Split('#');

                if (parts.Length != 2)
                {
                    return;
                }

                Guid sender;

                if (!Guid.TryParse(parts[0], out sender))
                {
                    return;
                }

                if (sender != instanceId)
                {
                    invalidationsReceived++;

                    cache.Remove(parts[1]);
                }
            }
            catch (Exception e)
            {
                logger.LogError(InfrastructureErrors.InvalidatingReceivedFailed, e, "Failed to receive invalidation message.");
            }
        }

        public void Invalidate(string key)
        {
            try
            {
                var message = string.Join("#", instanceId.ToString());

                subscriber.Publish(Channel, message);
            }
            catch (Exception e)
            {
                logger.LogError(InfrastructureErrors.InvalidatingReceivedFailed, e, "Failed to send invalidation message {0}", key);
            }
        }
    }
}
