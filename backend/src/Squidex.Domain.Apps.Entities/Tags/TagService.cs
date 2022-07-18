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
    public sealed class TagService : ITagService
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

        public TagService(IPersistenceFactory<State> persistenceFactory)
        {
            this.persistenceFactory = persistenceFactory;
        }

        public async Task RenameTagAsync(DomainId id, string group, string name, string newName,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name);
            Guard.NotNullOrEmpty(newName);

            var state = await GetStateAsync(id, group, ct);

            await state.UpdateAsync(s => s.Rename(name, newName), ct: ct);
        }

        public async Task RebuildTagsAsync(DomainId id, string group, TagsExport export,
            CancellationToken ct = default)
        {
            Guard.NotNull(export);

            var state = await GetStateAsync(id, group, ct);

            await state.UpdateAsync(s => s.Rebuild(export), ct: ct);
        }

        public async Task<Dictionary<string, string>> GetTagIdsAsync(DomainId id, string group, HashSet<string> names,
            CancellationToken ct = default)
        {
            Guard.NotNull(names);

            var state = await GetStateAsync(id, group, ct);

            return await state.UpdateAsync(s => s.GetTagIds(names), ct: ct);
        }

        public async Task<Dictionary<string, string>> DenormalizeTagsAsync(DomainId id, string group, HashSet<string> ids,
            CancellationToken ct = default)
        {
            Guard.NotNull(ids);

            var state = await GetStateAsync(id, group, ct);

            return await state.UpdateAsync(s => s.Denormalize(ids), ct: ct);
        }

        public async Task<Dictionary<string, string>> NormalizeTagsAsync(DomainId id, string group, HashSet<string>? names, HashSet<string>? ids,
            CancellationToken ct = default)
        {
            var state = await GetStateAsync(id, group, ct);

            return await state.UpdateAsync(s => s.Normalize(names, ids), ct: ct);
        }

        public async Task<TagsSet> GetTagsAsync(DomainId id, string group,
            CancellationToken ct = default)
        {
            var state = await GetStateAsync(id, group, ct);

            return state.Value.GetTags(state.Version);
        }

        public async Task<TagsExport> GetExportableTagsAsync(DomainId id, string group,
            CancellationToken ct = default)
        {
            var state = await GetStateAsync(id, group, ct);

            return state.Value.GetExportableTags();
        }

        public async Task ClearAsync(DomainId id, string group,
            CancellationToken ct = default)
        {
            var state = await GetStateAsync(id, group, ct);

            await state.ClearAsync(ct);
        }

        private async Task<SimpleState<State>> GetStateAsync(DomainId id, string group,
            CancellationToken ct)
        {
            var state = new SimpleState<State>(persistenceFactory, GetType(), $"{id}_{group}");

            await state.LoadAsync(ct);

            return state;
        }
    }
}
