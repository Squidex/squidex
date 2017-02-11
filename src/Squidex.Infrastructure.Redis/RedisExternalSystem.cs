// ==========================================================================
//  RedisExternalSystem.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis
{
    public sealed class RedisExternalSystem : IExternalSystem
    {
        private readonly IConnectionMultiplexer redis;

        public RedisExternalSystem(IConnectionMultiplexer redis)
        {
            Guard.NotNull(redis, nameof(redis));

            this.redis = redis;
        }

        public void CheckConnection()
        {
            try
            {
                redis.GetStatus();
            }
            catch (Exception e)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redis.Configuration}", e);
            }
        }
    }
}
