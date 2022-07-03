// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class TagGrain : GrainBase, ITagGrain
    {
        private readonly IGrainState<State> grainState;

        [CollectionName("Index_Tags")]
        public sealed class State : TagsExport
        {
        }

        private Dictionary<string, Tag> Tags
        {
            get => grainState.Value.Tags ??= new Dictionary<string, Tag>();
        }

        private Dictionary<string, string> Alias
        {
            get => grainState.Value.Alias ??= new Dictionary<string, string>();
        }

        public TagGrain(IGrainIdentity grainIdentity, IGrainState<State> grainState)
            : base(grainIdentity)
        {
            this.grainState = grainState;
        }

        public Task ClearAsync()
        {
            return grainState.ClearAsync();
        }

        public Task RebuildAsync(TagsExport export)
        {
            grainState.Value.Tags = export.Tags;
            grainState.Value.Alias = export.Alias;

            return grainState.WriteAsync();
        }

        public Task RenameTagAsync(string name, string newName)
        {
            Guard.NotNull(name);
            Guard.NotNull(newName);

            name = NormalizeName(name);

            var (_, tag) = FindTag(name);

            if (tag == null)
            {
                return Task.CompletedTask;
            }

            newName = NormalizeName(newName);

            tag.Name = newName;

            foreach (var alias in Alias.Where(x => x.Value == name).ToList())
            {
                Alias.Remove(alias.Key);

                if (alias.Key != newName)
                {
                    Alias[alias.Key] = newName;
                }
            }

            Alias[name] = newName;

            return grainState.WriteAsync();
        }

        public async Task<Dictionary<string, string>> NormalizeTagsAsync(HashSet<string>? names, HashSet<string>? ids)
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
                        if (Tags.TryGetValue(id, out var tagInfo))
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

            await grainState.WriteAsync();

            return result;
        }

        public Task<Dictionary<string, string>> GetTagIdsAsync(HashSet<string> names)
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

            return Task.FromResult(result);
        }

        public Task<Dictionary<string, string>> DenormalizeTagsAsync(HashSet<string> ids)
        {
            var result = new Dictionary<string, string>();

            foreach (var id in ids)
            {
                if (Tags.TryGetValue(id, out var tagInfo))
                {
                    result[id] = tagInfo.Name;
                }
            }

            return Task.FromResult(result);
        }

        public Task<TagsSet> GetTagsAsync()
        {
            var tags = Tags.Values.ToDictionary(x => x.Name, x => x.Count);

            return Task.FromResult(new TagsSet(tags, grainState.Version));
        }

        public Task<TagsExport> GetExportableTagsAsync()
        {
            return Task.FromResult(grainState.Value.Clone());
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
            if (Alias.TryGetValue(name, out var newName))
            {
                name = newName;
            }

            return Tags.FirstOrDefault(x => x.Value.Name == name);
        }
    }
}
