// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class GrainTagService : ITagService
    {
        private readonly IPersistenceFactory<State> persistenceFactory;

        [CollectionName("Index_Tags")]
        public sealed class State : TagsExport
        {
            public void Rebuild(TagsExport export)
            {
                Tags = export.Tags;

                Alias = export.Alias;
            }

            public void Rename(string name, string newName)
            {
                Guard.NotNull(name);
                Guard.NotNull(newName);

                name = NormalizeName(name);

                var (_, tag) = FindTag(name);

                if (tag == null)
                {
                    return;
                }

                newName = NormalizeName(newName);

                tag.Name = newName;

                if (Alias != null)
                {
                    foreach (var alias in Alias.Where(x => x.Value == name).ToList())
                    {
                        Alias.Remove(alias.Key);

                        if (alias.Key != newName)
                        {
                            Alias[alias.Key] = newName;
                        }
                    }
                }

                Alias ??= new Dictionary<string, string>();
                Alias[name] = newName;
            }

            public Dictionary<string, string> Normalize(HashSet<string>? names, HashSet<string>? ids)
            {
                var result = new Dictionary<string, string>();

                if (names != null)
                {
                    foreach (var tag in names)
                    {
                        var name = NormalizeName(tag);

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            result.Add(name, GetId(name, ids));
                        }
                    }
                }

                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        if (!result.ContainsValue(id))
                        {
                            if (Tags != null && Tags.TryGetValue(id, out var tagInfo))
                            {
                                tagInfo.Count--;

                                if (tagInfo.Count <= 0)
                                {
                                    Tags.Remove(id);
                                }
                            }
                        }
                    }
                }

                return result;
            }

            public Dictionary<string, string> GetTagIds(HashSet<string> names)
            {
                Guard.NotNull(names);

                var result = new Dictionary<string, string>();

                foreach (var tag in names)
                {
                    var name = NormalizeName(tag);

                    var (id, _) = FindTag(name);

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        result.Add(name, id);
                    }
                }

                return result;
            }

            public Dictionary<string, string> Denormalize(HashSet<string> ids)
            {
                var result = new Dictionary<string, string>();

                foreach (var id in ids)
                {
                    if (Tags?.TryGetValue(id, out var tagInfo) == true)
                    {
                        result[id] = tagInfo.Name;
                    }
                }

                return result;
            }

            public TagsSet GetTags(long version)
            {
                var tags = Tags?.Values.ToDictionary(x => x.Name, x => x.Count) ?? new Dictionary<string, int>();

                return new TagsSet(tags, version);
            }

            public TagsExport GetExportableTags()
            {
                var clone = Clone();

                return clone;
            }

            private string GetId(string name, HashSet<string>? ids)
            {
                var (id, tag) = FindTag(name);

                if (tag != null)
                {
                    if (ids == null || !ids.Contains(id))
                    {
                        tag.Count++;
                    }
                }
                else
                {
                    id = DomainId.NewGuid().ToString();

                    Tags ??= new Dictionary<string, Tag>();
                    Tags.Add(id, new Tag { Name = name });
                }

                return id;
            }

            private static string NormalizeName(string name)
            {
                return name.Trim().ToLowerInvariant();
            }

            private KeyValuePair<string, Tag> FindTag(string name)
            {
                if (Alias?.TryGetValue(name, out var newName) == true)
                {
                    name = newName;
                }

                return Tags?.FirstOrDefault(x => x.Value.Name == name) ?? default;
            }
        }

        public GrainTagService(IPersistenceFactory<State> persistenceFactory)
        {
            this.persistenceFactory = persistenceFactory;
        }

        public async Task RenameTagAsync(DomainId appId, string group, string name, string newName)
        {
            Guard.NotNullOrEmpty(name);
            Guard.NotNullOrEmpty(newName);

            var state = await GetStateAsync(appId, group);

            await state.UpdateAsync(s => s.Rename(name, newName));
        }

        public async Task RebuildTagsAsync(DomainId appId, string group, TagsExport export)
        {
            Guard.NotNull(export);

            var state = await GetStateAsync(appId, group);

            await state.UpdateAsync(s => s.Rebuild(export));
        }

        public async Task<Dictionary<string, string>> GetTagIdsAsync(DomainId appId, string group, HashSet<string> names)
        {
            Guard.NotNull(names);

            var state = await GetStateAsync(appId, group);

            return await state.UpdateAsync(s => s.GetTagIds(names));
        }

        public async Task<Dictionary<string, string>> DenormalizeTagsAsync(DomainId appId, string group, HashSet<string> ids)
        {
            Guard.NotNull(ids);

            var state = await GetStateAsync(appId, group);

            return await state.UpdateAsync(s => s.Denormalize(ids));
        }

        public async Task<Dictionary<string, string>> NormalizeTagsAsync(DomainId appId, string group, HashSet<string>? names, HashSet<string>? ids)
        {
            var state = await GetStateAsync(appId, group);

            return await state.UpdateAsync(s => s.Normalize(names, ids));
        }

        public async Task<TagsSet> GetTagsAsync(DomainId appId, string group)
        {
            var state = await GetStateAsync(appId, group);

            return state.Value.GetTags(state.Version);
        }

        public async Task<TagsExport> GetExportableTagsAsync(DomainId appId, string group)
        {
            var state = await GetStateAsync(appId, group);

            return state.Value.GetExportableTags();
        }

        public async Task ClearAsync(DomainId appId, string group)
        {
            var state = await GetStateAsync(appId, group);

            await state.ClearAsync();
        }

        private async Task<SimpleState<State>> GetStateAsync(DomainId appId, string group)
        {
            var state = new SimpleState<State>(persistenceFactory, GetType(), $"{appId}_{group}");

            await state.LoadAsync();

            return state;
        }
    }
}
