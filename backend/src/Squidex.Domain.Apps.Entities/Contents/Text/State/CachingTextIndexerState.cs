// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class CachingTextIndexerState : ITextIndexerState
    {
        private readonly ITextIndexerState inner;
        private LRUCache<Guid, Tuple<TextContentState?>> cache = new LRUCache<Guid, Tuple<TextContentState?>>(1000);

        public CachingTextIndexerState(ITextIndexerState inner)
        {
            Guard.NotNull(inner);

            this.inner = inner;
        }

        public async Task ClearAsync()
        {
            await inner.ClearAsync();

            cache = new LRUCache<Guid, Tuple<TextContentState?>>(1000);
        }

        public async Task<TextContentState?> GetAsync(Guid contentId)
        {
            if (cache.TryGetValue(contentId, out var value))
            {
                return value.Item1;
            }

            var result = await inner.GetAsync(contentId);

            cache.Set(contentId, Tuple.Create(result));

            return result;
        }

        public Task SetAsync(TextContentState state)
        {
            Guard.NotNull(state);

            cache.Set(state.ContentId, Tuple.Create<TextContentState?>(state));

            return inner.SetAsync(state);
        }

        public Task RemoveAsync(Guid contentId)
        {
            cache.Set(contentId, Tuple.Create<TextContentState?>(null));

            return inner.RemoveAsync(contentId);
        }
    }
}
