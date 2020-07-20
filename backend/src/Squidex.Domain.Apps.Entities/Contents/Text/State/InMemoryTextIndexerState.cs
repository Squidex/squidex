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
        private readonly Dictionary<(DomainId, DomainId), TextContentState> states = new Dictionary<(DomainId, DomainId), TextContentState>();

        public Task ClearAsync()
        {
            states.Clear();

            return Task.CompletedTask;
        }

        public Task<TextContentState?> GetAsync(DomainId appId, DomainId contentId)
        {
            if (states.TryGetValue((appId, contentId), out var result))
            {
                return Task.FromResult<TextContentState?>(result);
            }

            return Task.FromResult<TextContentState?>(null);
        }

        public Task SetAsync(DomainId appId, TextContentState state)
        {
            states[(appId, state.ContentId)] = state;

            return Task.CompletedTask;
        }

        public Task RemoveAsync(DomainId appId, DomainId contentId)
        {
            states.Remove((appId, contentId));

            return Task.CompletedTask;
        }
    }
}
