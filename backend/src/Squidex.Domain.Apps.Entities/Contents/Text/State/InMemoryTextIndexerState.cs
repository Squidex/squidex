// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class InMemoryTextIndexerState : ITextIndexerState
    {
        private readonly Dictionary<Guid, ContentState> states = new Dictionary<Guid, ContentState>();

        public Task<ContentState?> GetAsync(Guid contentId)
        {
            if (states.TryGetValue(contentId, out var result))
            {
                return Task.FromResult<ContentState?>(result);
            }

            return Task.FromResult<ContentState?>(null);
        }

        public Task RemoveAsync(Guid contentId)
        {
            states.Remove(contentId);

            return TaskHelper.Done;
        }

        public Task SetAsync(ContentState state)
        {
            states[state.ContentId] = state;

            return TaskHelper.Done;
        }
    }
}
