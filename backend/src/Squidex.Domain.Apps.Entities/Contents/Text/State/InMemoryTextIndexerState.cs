// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class InMemoryTextIndexerState : ITextIndexerState
    {
        private readonly Dictionary<DomainId, TextContentState> states = new Dictionary<DomainId, TextContentState>();

        public Task ClearAsync()
        {
            states.Clear();

            return Task.CompletedTask;
        }

        public Task<TextContentState?> GetAsync(DomainId contentId)
        {
            if (states.TryGetValue(contentId, out var result))
            {
                return Task.FromResult<TextContentState?>(result);
            }

            return Task.FromResult<TextContentState?>(null);
        }

        public Task RemoveAsync(DomainId contentId)
        {
            states.Remove(contentId);

            return Task.CompletedTask;
        }

        public Task SetAsync(TextContentState state)
        {
            states[state.ContentId] = state;

            return Task.CompletedTask;
        }
    }
}
