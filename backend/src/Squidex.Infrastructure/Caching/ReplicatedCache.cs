// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.Caching
{
    public sealed class ReplicatedCache : IReplicatedCache
    {
        private readonly Guid instanceId = Guid.NewGuid();
        private readonly IMemoryCache memoryCache;
        private readonly IPubSub pubSub;

        public class InvalidateMessage
        {
            public Guid Source { get; set; }

            public string Key { get; set; }
        }

        public ReplicatedCache(IMemoryCache memoryCache, IPubSub pubSub)
        {
            Guard.NotNull(memoryCache, nameof(memoryCache));
            Guard.NotNull(pubSub, nameof(pubSub));

            this.memoryCache = memoryCache;

            this.pubSub = pubSub;
            this.pubSub.Subscribe(OnMessage);
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
            memoryCache.Set(key, value, expiration);

            if (invalidate)
            {
                Invalidate(key);
            }
        }

        public void Remove(string key)
        {
            memoryCache.Remove(key);

            Invalidate(key);
        }

        public bool TryGetValue(string key, out object? value)
        {
            return memoryCache.TryGetValue(key, out value);
        }

        private void Invalidate(string key)
        {
            pubSub.Publish(new InvalidateMessage { Key = key, Source = instanceId });
        }
    }
}
