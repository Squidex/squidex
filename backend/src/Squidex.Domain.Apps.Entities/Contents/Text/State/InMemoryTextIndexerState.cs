// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class InMemoryTextIndexerState : ITextIndexerState
    {
        private readonly Dictionary<Guid, TextContentState> states = new Dictionary<Guid, TextContentState>();

        public Task ClearAsync()
        {
            states.Clear();

            return Task.CompletedTask;
        }

        public Task<Dictionary<Guid, TextContentState>> GetAsync(HashSet<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var result = new Dictionary<Guid, TextContentState>();

            foreach (var id in ids)
            {
                if (states.TryGetValue(id, out var state))
                {
                    result.Add(id, state);
                }
            }

            return Task.FromResult(result);
        }

        public Task SetAsync(List<TextContentState> updates)
        {
            Guard.NotNull(updates, nameof(updates));

            foreach (var update in updates)
            {
                if (update.IsDeleted)
                {
                    states.Remove(update.ContentId);
                }
                else
                {
                    states[update.ContentId] = update;
                }
            }

            return Task.CompletedTask;
        }
    }
}
