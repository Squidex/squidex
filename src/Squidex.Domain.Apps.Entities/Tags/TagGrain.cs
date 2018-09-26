// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class TagGrain : GrainOfString, ITagGrain
    {
        private readonly IStore<string> store;
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("Index_Tags")]
        public sealed class State
        {
            public TagSet Tags { get; set; } = new TagSet();
        }

        public TagGrain(IStore<string> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(string key)
        {
            persistence = store.WithSnapshots<TagGrain, State, string>(key, s =>
            {
                state = s;
            });

            return persistence.ReadAsync();
        }

        public Task ClearAsync()
        {
            state = new State();

            return persistence.DeleteAsync();
        }

        public Task RebuildAsync(TagSet tags)
        {
            state.Tags = tags;

            return persistence.WriteSnapshotAsync(state);
        }

        public async Task<Dictionary<string, string>> NormalizeTagsAsync(HashSet<string> names, HashSet<string> ids)
        {
            var result = new Dictionary<string, string>();

            if (names != null)
            {
                foreach (var tag in names)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        var tagName = tag.ToLowerInvariant();
                        var tagId = string.Empty;

                        var found = state.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, tagName, StringComparison.OrdinalIgnoreCase));

                        if (found.Value != null)
                        {
                            tagId = found.Key;

                            if (ids == null || !ids.Contains(tagId))
                            {
                                found.Value.Count++;
                            }
                        }
                        else
                        {
                            tagId = Guid.NewGuid().ToString();

                            state.Tags.Add(tagId, new Tag { Name = tagName });
                        }

                        result.Add(tagName, tagId);
                    }
                }
            }

            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (!result.ContainsValue(id))
                    {
                        if (state.Tags.TryGetValue(id, out var tagInfo))
                        {
                            tagInfo.Count--;

                            if (tagInfo.Count <= 0)
                            {
                                state.Tags.Remove(id);
                            }
                        }
                    }
                }
            }

            await persistence.WriteSnapshotAsync(state);

            return result;
        }

        public Task<Dictionary<string, string>> GetTagIdsAsync(HashSet<string> names)
        {
            var result = new Dictionary<string, string>();

            foreach (var name in names)
            {
                var id = state.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase)).Key;

                if (!string.IsNullOrWhiteSpace(id))
                {
                    result.Add(name, id);
                }
            }

            return Task.FromResult(result);
        }

        public Task<Dictionary<string, string>> DenormalizeTagsAsync(HashSet<string> ids)
        {
            var result = new Dictionary<string, string>();

            foreach (var id in ids)
            {
                if (state.Tags.TryGetValue(id, out var tagInfo))
                {
                    result[id] = tagInfo.Name;
                }
            }

            return Task.FromResult(result);
        }

        public Task<Dictionary<string, int>> GetTagsAsync()
        {
            return Task.FromResult(state.Tags.Values.ToDictionary(x => x.Name, x => x.Count));
        }

        public Task<TagSet> GetExportableTagsAsync()
        {
            return Task.FromResult(state.Tags);
        }
    }
}
