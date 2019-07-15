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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class TagGrain : GrainOfString<TagGrain.GrainState>, ITagGrain
    {
        [CollectionName("Index_Tags")]
        public sealed class GrainState
        {
            public TagsExport Tags { get; set; } = new TagsExport();
        }

        public TagGrain(IStore<string> store)
            : base(store)
        {
        }

        public Task ClearAsync()
        {
            return ClearStateAsync();
        }

        public Task RebuildAsync(TagsExport tags)
        {
            State.Tags = tags;

            return WriteStateAsync();
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

                        var found = State.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, tagName, StringComparison.OrdinalIgnoreCase));

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

                            State.Tags.Add(tagId, new Tag { Name = tagName });
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
                        if (State.Tags.TryGetValue(id, out var tagInfo))
                        {
                            tagInfo.Count--;

                            if (tagInfo.Count <= 0)
                            {
                                State.Tags.Remove(id);
                            }
                        }
                    }
                }
            }

            await WriteStateAsync();

            return result;
        }

        public Task<Dictionary<string, string>> GetTagIdsAsync(HashSet<string> names)
        {
            var result = new Dictionary<string, string>();

            foreach (var name in names)
            {
                var id = State.Tags.FirstOrDefault(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase)).Key;

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
                if (State.Tags.TryGetValue(id, out var tagInfo))
                {
                    result[id] = tagInfo.Name;
                }
            }

            return Task.FromResult(result);
        }

        public Task<TagsSet> GetTagsAsync()
        {
            var tags = State.Tags.Values.ToDictionary(x => x.Name, x => x.Count);

            return Task.FromResult(new TagsSet(tags, Persistence.Version));
        }

        public Task<TagsExport> GetExportableTagsAsync()
        {
            return Task.FromResult(State.Tags);
        }
    }
}
