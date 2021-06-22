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
        private readonly IGrainState<State> state;

        [CollectionName("Index_Tags")]
        public sealed class State
        {
            public TagsExport Tags { get; set; } = new TagsExport();
        }

        public TagGrain(IGrainState<State> state)
        {
            this.state = state;
        }

        public Task ClearAsync()
        {
            return state.ClearAsync();
        }

        public Task RebuildAsync(TagsExport tags)
        {
            state.Value.Tags = tags;

            return state.WriteAsync();
        }

        public async Task<Dictionary<string, string>> NormalizeTagsAsync(HashSet<string>? names, HashSet<string>? ids)
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

                        var (key, value) = state.Value.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, tagName, StringComparison.OrdinalIgnoreCase));

                        if (value != null)
                        {
                            tagId = key;

                            if (ids == null || !ids.Contains(tagId))
                            {
                                value.Count++;
                            }
                        }
                        else
                        {
                            tagId = DomainId.NewGuid().ToString();

                            state.Value.Tags.Add(tagId, new Tag { Name = tagName });
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
                        if (state.Value.Tags.TryGetValue(id, out var tagInfo))
                        {
                            tagInfo.Count--;

                            if (tagInfo.Count <= 0)
                            {
                                state.Value.Tags.Remove(id);
                            }
                        }
                    }
                }
            }

            await state.WriteAsync();

            return result;
        }

        public Task<Dictionary<string, string>> GetTagIdsAsync(HashSet<string> names)
        {
            var result = new Dictionary<string, string>();

            foreach (var name in names)
            {
                var id = state.Value.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase)).Key;

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
                if (state.Value.Tags.TryGetValue(id, out var tagInfo))
                {
                    result[id] = tagInfo.Name;
                }
            }

            return Task.FromResult(result);
        }

        public Task<TagsSet> GetTagsAsync()
        {
            var tags = state.Value.Tags.Values.ToDictionary(x => x.Name, x => x.Count);

            return Task.FromResult(new TagsSet(tags, state.Version));
        }

        public Task<TagsExport> GetExportableTagsAsync()
        {
            return Task.FromResult(state.Value.Tags);
        }
    }
}
