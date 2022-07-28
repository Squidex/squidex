﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks.Dataflow;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class TagService : ITagService
    {
        private readonly IPersistenceFactory<State> persistenceFactory;

        [CollectionName("Index_Tags")]
        public sealed class State : TagsExport, IOnRead
        {
            public ValueTask OnReadAsync()
            {
                if (Tags == null)
                {
                    Tags = new Dictionary<string, Tag>();
                }

                if (Alias == null)
                {
                    Alias = new Dictionary<string, string>();
                }

                return default;
            }

            public bool Rebuild(TagsExport export)
            {
                if (export.Tags != null)
                {
                    Tags = export.Tags;
                }

                if (export.Alias != null)
                {
                    Alias = export.Alias;
                }

                return true;
            }

            public bool Clear()
            {
                var isChanged = false;

                foreach (var (_, tag) in Tags)
                {
                    isChanged = tag.Count > 0;

                    tag.Count = 0;
                }

                return isChanged;
            }

            public bool Rename(string name, string newName)
            {
                name = NormalizeName(name);

                if (!TryGetTag(name, out var tag))
                {
                    return false;
                }

                newName = NormalizeName(newName);

                if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                tag.Value.Name = newName;

                foreach (var alias in Alias.Where(x => x.Value == name).ToList())
                {
                    Alias.Remove(alias.Key);

                    if (alias.Key != tag.Value.Name)
                    {
                        Alias[alias.Key] = tag.Value.Name;
                    }
                }

                return true;
            }

            public bool Update(Dictionary<string, int> updates)
            {
                var isChanged = false;

                foreach (var (id, update) in updates)
                {
                    if (update != 0 && Tags.TryGetValue(id, out var tag))
                    {
                        var newCount = Math.Max(0, tag.Count + update);

                        if (newCount != tag.Count)
                        {
                            tag.Count = newCount;

                            isChanged = true;
                        }
                    }
                }

                return isChanged;
            }

            public (bool, Dictionary<string, string>) GetIds(HashSet<string> names)
            {
                var tagIds = new Dictionary<string, string>();

                var isChanged = false;

                foreach (var name in names.Select(NormalizeName))
                {
                    if (TryGetTag(name, out var tag))
                    {
                        tagIds[name] = tag.Key;
                    }
                    else
                    {
                        var id = Guid.NewGuid().ToString();

                        Tags[id] = new Tag { Name = name };
                        tagIds[name] = id;

                        isChanged = true;
                    }
                }

                return (isChanged, tagIds);
            }

            public Dictionary<string, string> GetNames(HashSet<string> ids)
            {
                var tagNames = new Dictionary<string, string>();

                foreach (var id in ids)
                {
                    if (Tags.TryGetValue(id, out var tagInfo))
                    {
                        tagNames[id] = tagInfo.Name;
                    }
                }

                return tagNames;
            }

            public TagsSet GetTags(long version)
            {
                var clone = Tags.Values.ToDictionary(x => x.Name, x => x.Count);

                return new TagsSet(clone, version);
            }

            private static string NormalizeName(string name)
            {
                return name.Trim().ToLowerInvariant();
            }

            private bool TryGetTag(string name, out KeyValuePair<string, Tag> result)
            {
                result = default;

                if (Alias.TryGetValue(name, out var newName))
                {
                    name = newName;
                }

                var found = Tags.FirstOrDefault(x => x.Value.Name == name);

                if (found.Value != null)
                {
                    result = new KeyValuePair<string, Tag>(found.Key, found.Value);
                    return true;
                }

                return false;
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

            return await state.UpdateAsync(s => s.GetIds(names), ct: ct);
        }

        public async Task<Dictionary<string, string>> GetTagNamesAsync(DomainId id, string group, HashSet<string> ids,
            CancellationToken ct = default)
        {
            Guard.NotNull(ids);

            var state = await GetStateAsync(id, group, ct);

            return state.Value.GetNames(ids);
        }

        public async Task UpdateAsync(DomainId id, string group, Dictionary<string, int> update,
            CancellationToken ct = default)
        {
            var state = await GetStateAsync(id, group, ct);

            await state.UpdateAsync(s => s.Update(update), ct: ct);
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

            return state.Value;
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

        public async Task ClearAsync(
            CancellationToken ct = default)
        {
            var writerBlock = new ActionBlock<SnapshotResult<State>[]>(async batch =>
            {
                try
                {
                    var isChanged = !batch.All(x => !x.Value.Clear());

                    if (isChanged)
                    {
                        var jobs = batch.Select(x => new SnapshotWriteJob<State>(x.Key, x.Value, x.Version));

                        await persistenceFactory.Snapshots.WriteManyAsync(jobs, ct);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Dataflow swallows operation cancelled exception.
                    throw new AggregateException(ex);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 2,
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = 1,
            });

            // Create batches of 500 items to clear the tag count for better performance.
            var batchBlock = new BatchBlock<SnapshotResult<State>>(500, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = 500
            });

            batchBlock.BidirectionalLinkTo(writerBlock);

            await foreach (var state in persistenceFactory.Snapshots.ReadAllAsync(ct))
            {
                // Uses back-propagation to not query additional items from the database, when queue is full.
                await batchBlock.SendAsync(state, ct);
            }

            batchBlock.Complete();

            await writerBlock.Completion;
        }
    }
}
