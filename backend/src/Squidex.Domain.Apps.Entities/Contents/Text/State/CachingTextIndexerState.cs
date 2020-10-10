﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Caching;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class CachingTextIndexerState : ITextIndexerState
    {
        private readonly ITextIndexerState inner;
        private readonly LRUCache<Guid, Tuple<TextContentState?>> cache = new LRUCache<Guid, Tuple<TextContentState?>>(10000);

        public CachingTextIndexerState(ITextIndexerState inner)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public async Task ClearAsync()
        {
            await inner.ClearAsync();

            cache.Clear();
        }

        public async Task<Dictionary<Guid, TextContentState>> GetAsync(HashSet<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var missingIds = new HashSet<Guid>();

            var result = new Dictionary<Guid, TextContentState>();

            foreach (var id in ids)
            {
                if (cache.TryGetValue(id, out var state))
                {
                    if (state.Item1 != null)
                    {
                        result[id] = state.Item1;
                    }
                }
                else
                {
                    missingIds.Add(id);
                }
            }

            if (missingIds.Count > 0)
            {
                var fromInner = await inner.GetAsync(missingIds);

                foreach (var (id, state) in fromInner)
                {
                    result[id] = state;
                }

                foreach (var id in missingIds)
                {
                    var state = fromInner.GetOrDefault(id);

                    cache.Set(id, Tuple.Create<TextContentState?>(state));
                }
            }

            return result;
        }

        public Task SetAsync(List<TextContentState> updates)
        {
            Guard.NotNull(updates, nameof(updates));

            foreach (var update in updates)
            {
                if (update.IsDeleted)
                {
                    cache.Set(update.ContentId, Tuple.Create<TextContentState?>(null));
                }
                else
                {
                    cache.Set(update.ContentId, Tuple.Create<TextContentState?>(update));
                }
            }

            return inner.SetAsync(updates);
        }
    }
}
