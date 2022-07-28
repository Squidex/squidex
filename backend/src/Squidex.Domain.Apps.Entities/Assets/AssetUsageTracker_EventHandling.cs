// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker : IEventConsumer
    {
        private IMemoryCache memoryCache;

        public int BatchSize
        {
            get => 1;
        }

        public int BatchDelay
        {
            get => 1000;
        }

        public string Name
        {
            get => GetType().Name;
        }

        public string EventsFilter
        {
            get => "^asset-";
        }

        private void ClearCache()
        {
            memoryCache?.Dispose();
            memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        public async Task ClearAsync()
        {
            // Will not remove data, but reset alls counts to zero.
            await tagService.ClearAsync();

            // Also clear the store and cache, because otherwise we would use data from the future when querying old tags.
            ClearCache();

            await store.ClearAsync();

            // Use a well defined prefix query for the deletion to improve performance.
            await usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_Assets");
        }

        public async Task On(IEnumerable<Envelope<IEvent>> events)
        {
            foreach (var @event in events)
            {
                // Usage tracking is done in the backgroud, therefore we do no use any batching.
                await TrackUsageAsync(@event);
            }

            // Event consumers should only do one task, but too many consumers also hurt performance.
            await AddTagsAsync(events);
        }

        private async Task AddTagsAsync(IEnumerable<Envelope<IEvent>> events)
        {
            var tagsPerApp = new Dictionary<DomainId, Dictionary<string, int>>();
            var tagsPerAsset = new Dictionary<DomainId, State>();

            void AddTagsToStore(DomainId appId, HashSet<string>? tagIds, int count)
            {
                if (tagIds != null)
                {
                    var perApp = tagsPerApp.GetOrAddNew(appId);

                    foreach (var tag in tagIds)
                    {
                        perApp[tag] = perApp.GetOrAddDefault(tag) + count;
                    }
                }
            }

            void AddTagsToCache(DomainId key, HashSet<string>? tags, long version)
            {
                // Also cache null tags to keep them in as valid state in cache and store.
                var state = new State { Tags = tags };

                // Write tags to a buffer so that we can write them to a store in batches.
                tagsPerAsset[key] = state;

                // Write to the cache immediately, to be available for the next event. Use a relatively long cache time for live updates.
                memoryCache.Set(key, state, TimeSpan.FromHours(1));
            }

            foreach (var @event in events)
            {
                var typedEvent = (AssetEvent)@event.Payload;

                var appId = typedEvent.AppId.Id;
                var assetId = typedEvent.AssetId;
                var assetKey = @event.Headers.AggregateId();
                var version = @event.Headers.EventStreamNumber();

                switch (typedEvent)
                {
                    case AssetCreated assetCreated:
                        {
                            AddTagsToStore(appId, assetCreated.Tags, 1);

                            AddTagsToCache(assetKey, assetCreated.Tags, version);
                            break;
                        }

                    case AssetAnnotated assetAnnotated when assetAnnotated.Tags != null:
                        {
                            var oldTags = await GetAndUpdateOldTagsAsync(appId, assetId, assetKey, version, default);

                            AddTagsToStore(appId, assetAnnotated.Tags, 1);
                            AddTagsToStore(appId, oldTags, -1);

                            AddTagsToCache(assetKey, assetAnnotated.Tags, version);
                            break;
                        }

                    case AssetDeleted assetDeleted:
                        {
                            var oldTags = await GetAndUpdateOldTagsAsync(appId, assetId, assetKey, version, default);

                            AddTagsToStore(appId, oldTags, -1);
                            break;
                        }
                }
            }

            // There is no good solution for batching anyway, so there is no need to build a method for that.
            foreach (var (appId, updates) in tagsPerApp)
            {
                await tagService.UpdateAsync(appId, TagGroups.Assets, updates);
            }

            await store.WriteManyAsync(tagsPerAsset.Select(x => new SnapshotWriteJob<State>(x.Key, x.Value, 0)));
        }

        private async Task<HashSet<string>?> GetAndUpdateOldTagsAsync(DomainId appId, DomainId assetId, DomainId key, long version,
            CancellationToken ct)
        {
            // Store the latest tags in memory for fast access.
            if (memoryCache.TryGetValue<State>(key, out var state))
            {
                return state.Tags;
            }

            var stored = await store.ReadAsync(key, ct);

            // Stored state can be null, if not serialized yet.
            if (stored.Value != null)
            {
                return stored.Value.Tags;
            }

            // This will replay a lot of events, so it is the slowest alternative.
            var previousAsset = await assetLoader.GetAsync(appId, assetId, version - 1, ct);

            return previousAsset?.Tags;
        }

        private Task TrackUsageAsync(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    return UpdateSizeAsync(assetCreated.AppId.Id, GetDate(@event), assetCreated.FileSize, 1);

                case AssetUpdated assetUpdated:
                    return UpdateSizeAsync(assetUpdated.AppId.Id, GetDate(@event), assetUpdated.FileSize, 0);

                case AssetDeleted assetDeleted:
                    return UpdateSizeAsync(assetDeleted.AppId.Id, GetDate(@event), -assetDeleted.DeletedSize, -1);
            }

            return Task.CompletedTask;
        }

        private static DateTime GetDate(Envelope<IEvent> @event)
        {
            return @event.Headers.Timestamp().ToDateTimeUtc().Date;
        }

        private Task UpdateSizeAsync(DomainId appId, DateTime date, long size, long count)
        {
            var counters = new Counters
            {
                [CounterTotalSize] = size,
                [CounterTotalCount] = count
            };

            var appKey = GetKey(appId);

            return Task.WhenAll(
                usageTracker.TrackAsync(date, appKey, null, counters),
                usageTracker.TrackAsync(SummaryDate, appKey, null, counters));
        }

        private static string GetKey(DomainId appId)
        {
            return $"{appId}_Assets";
        }
    }
}
