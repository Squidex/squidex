// ==========================================================================
//  RedisEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.CQRS.Events;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis
{ 
    public sealed class RedisEventNotifier : IEventNotifier
    {
        private const string Channel = "SquidexEventNotifications";
        private readonly InMemoryEventNotifier inMemoryNotifier = new InMemoryEventNotifier();
        private readonly ISubscriber subscriber;
        private readonly ILogger<RedisEventNotifier> logger;

        public RedisEventNotifier(IConnectionMultiplexer redis, ILogger<RedisEventNotifier> logger)
        {
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(logger, nameof(logger));

            subscriber = redis.GetSubscriber();
            subscriber.Subscribe(Channel, (channel, value) => HandleInvalidation());

            this.logger = logger;
        }

        private void HandleInvalidation()
        {
            try
            {
                inMemoryNotifier.NotifyEventsStored();
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.InvalidatingReceivedFailed, ex, "Failed to receive invalidation message.");
            }
        }

        public void NotifyEventsStored()
        {
            try
            {
                subscriber.Publish(Channel, RedisValue.Null);
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.InvalidatingReceivedFailed, ex, "Failed to send invalidation message");
            }
        }

        public void Subscribe(Action handler)
        {
            inMemoryNotifier.Subscribe(handler);
        }
    }
}
