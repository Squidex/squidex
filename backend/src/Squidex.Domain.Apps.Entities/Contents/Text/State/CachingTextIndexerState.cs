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
        private LRUCache<(DomainId, DomainId), Tuple<TextContentState?>> cache = new LRUCache<(DomainId, DomainId), Tuple<TextContentState?>>(1000);

        public CachingTextIndexerState(ITextIndexerState inner)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public async Task ClearAsync()
        {
            await inner.ClearAsync();

            cache = new LRUCache<(DomainId, DomainId), Tuple<TextContentState?>>(1000);
        }

        public async Task<TextContentState?> GetAsync(DomainId appId, DomainId contentId)
        {
            if (cache.TryGetValue((appId, contentId), out var value))
            {
                return value.Item1;
            }

            var result = await inner.GetAsync(appId, contentId);

            cache.Set((appId, contentId), Tuple.Create(result));

            return result;
        }

        public Task SetAsync(DomainId appId, TextContentState state)
        {
            Guard.NotNull(state, nameof(state));

            cache.Set((appId, state.ContentId), Tuple.Create<TextContentState?>(state));

            return inner.SetAsync(appId, state);
        }

        public Task RemoveAsync(DomainId appId, DomainId contentId)
        {
            cache.Set((appId, contentId), Tuple.Create<TextContentState?>(null));

            return inner.RemoveAsync(appId, contentId);
        }
    }
}
