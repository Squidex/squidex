// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Caching
{
    public sealed class ReplicatedCache : IReplicatedCache
    {
        private readonly Guid instanceId = Guid.NewGuid();
        private readonly IMemoryCache memoryCache;
        private readonly IPubSub pubSub;
        private readonly ReplicatedCacheOptions options;

        public class InvalidateMessage
        {
            public Guid Source { get; set; }

            public string Key { get; set; }
        }

        public ReplicatedCache(IMemoryCache memoryCache, IPubSub pubSub, IOptions<ReplicatedCacheOptions> options)
        {
            Guard.NotNull(memoryCache, nameof(memoryCache));
            Guard.NotNull(pubSub, nameof(pubSub));
            Guard.NotNull(options, nameof(options));

            this.memoryCache = memoryCache;

            this.pubSub = pubSub;

            if (options.Value.Enable)
            {
                this.pubSub.Subscribe(OnMessage);
            }

            this.options = options.Value;
        }

        private void OnMessage(object message)
        {
            if (message is InvalidateMessage invalidate && invalidate.Source != instanceId)
            {
                memoryCache.Remove(invalidate.Key);
            }
        }

        public void Add(string key, object? value, TimeSpan expiration, bool invalidate)
        {
            if (!options.Enable)
            {
                return;
            }

            memoryCache.Set(key, value, expiration);

            if (invalidate)
            {
                Invalidate(key);
            }
        }

        public void Remove(string key)
        {
            if (!options.Enable)
            {
                return;
            }

            memoryCache.Remove(key);

            Invalidate(key);
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (!options.Enable)
            {
                value = null;

                return false;
            }

            return memoryCache.TryGetValue(key, out value);
        }

        private void Invalidate(string key)
        {
            if (!options.Enable)
            {
                return;
            }

            pubSub.Publish(new InvalidateMessage { Key = key, Source = instanceId });
        }
    }
}
