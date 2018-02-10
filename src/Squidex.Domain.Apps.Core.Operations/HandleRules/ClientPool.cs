// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Domain.Apps.Core.HandleRules
{
    internal sealed class ClientPool<TKey, TClient>
    {
        private static readonly TimeSpan TTL = TimeSpan.FromMinutes(30);
        private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Func<TKey, TClient> factory;

        public ClientPool(Func<TKey, TClient> factory)
        {
            this.factory = factory;
        }

        public TClient GetClient(TKey key)
        {
            if (!memoryCache.TryGetValue<TClient>(key, out var client))
            {
                client = factory(key);

                memoryCache.Set(key, client, TTL);
            }

            return client;
        }
    }
}
