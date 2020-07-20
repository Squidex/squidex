// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Extensions.Actions
{
    internal sealed class ClientPool<TKey, TClient>
    {
        private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(30);
        private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Func<TKey, Task<TClient>> factory;

        public ClientPool(Func<TKey, TClient> factory)
        {
            this.factory = x => Task.FromResult(factory(x));
        }

        public ClientPool(Func<TKey, Task<TClient>> factory)
        {
            this.factory = factory;
        }

        public async Task<TClient> GetClientAsync(TKey key)
        {
            if (!memoryCache.TryGetValue<TClient>(key, out var client))
            {
                client = await factory(key);

                memoryCache.Set(key, client, TimeToLive);
            }

            return client;
        }
    }
}
