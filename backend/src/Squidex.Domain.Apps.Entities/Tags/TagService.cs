// ==========================================================================
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

namespace Squidex.Domain.Apps.Entities.Tags;

public sealed class TagService : ITagService
{
    private readonly IPersistenceFactory<State> persistenceFactory;

    [CollectionName("Index_Tags")]
    public sealed class State : TagsExport, IOnRead
    {
        public ValueTask OnReadAsync()
        {
            // Tags should never be null, but it might happen due of bugs.
            Tags ??= new Dictionary<string, Tag>();

            // Alias can be null, because it was not part of the initial release.
            Alias ??= new Dictionary<string, string>();

            return default;
        }

        public bool Rebuild(TagsExport export)
        {
            // Tags should never be null, but it might happen due of bugs.
            if (export.Tags != null)
            {
                Tags = export.Tags;
            }

            // Alias can be null, because it was not part of the initial release.
            if (export.Alias != null)
            {
                Alias = export.Alias;
            }

            return true;
        }

        public bool Clear()
        {
            var isChanged = false;

            // Clear only resets the counts to zero, because we have no other source for tag names.
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

            if (!TryGetTag(name, out var tag, false))
            {
                return false;
            }

            // Avoid the normalization of the new name, if the old name does not exist.
            newName = NormalizeName(newName);

            if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (TryGetTag(newName, out var newTag, false))
            {
                // Merge both tags by adding up the count.
                newTag.Info.Count += tag.Info.Count;

                // Remove one of the tags.
                Tags.Remove(tag.Id);
            }
            else
            {
                tag.Info.Name = newName;
            }

            foreach (var alias in Alias.Where(x => x.Value == name).ToList())
            {
                // Remove the mapping to the old name.
                Alias.Remove(alias.Key);

                // If the tag has been named back to the original name, we do not need the mapping anymore.
                if (alias.Key != newName)
                {
                    // Create a new mapping to the new name.
                    Alias[alias.Key] = newName;
                }
            }

            // Create a new mapping to the new name.
            Alias[name] = newName;

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
                    // If the tag exists, return the ID.
                    tagIds[name] = tag.Id;
                }
                else
                {
                    // If the tag does not exist create a new one with a random ID.
                    var id = Guid.NewGuid().ToString();

                    Tags[id] = new Tag { Name = name };
                    tagIds[name] = id;

                    // Track that something has changed and the state needs to be written.
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
                if (Tags.TryGetValue(id, out var tag))
                {
                    tagNames[id] = tag.Name;
                }
            }

            return tagNames;
        }

        public TagsSet GetTags(long version)
        {
            var result = new Dictionary<string, int>();

            foreach (var tag in Tags.Values)
            {
                // We have changed the normalization logic, therefore some names are not up to date.
                var name = NormalizeName(tag.Name);

                // An old bug could have produced duplicate names.
                result[name] = result.GetValueOrDefault(name) + tag.Count;
            }

            return new TagsSet(result, version);
        }

        private static string NormalizeName(string name)
        {
            return name.TrimNonLetterOrDigit().ToLowerInvariant();
        }

        private bool TryGetTag(string name, out (string Id, Tag Info)result, bool useAlias = true)
        {
            result = default!;

            // If the tag has been renamed we create a mapping from the old name to the new name.
            if (useAlias && Alias.TryGetValue(name, out var newName))
            {
                name = newName;
            }

            var found = Tags.FirstOrDefault(x => x.Value.Name == name);

            if (found.Value != null)
            {
                result = (found.Key, found.Value);
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
