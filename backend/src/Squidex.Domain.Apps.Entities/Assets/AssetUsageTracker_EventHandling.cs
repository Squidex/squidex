// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.UsageTracking;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker : IEventConsumer
    {
        public int BatchSize
        {
            get => 1000;
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

        public bool CanClear
        {
            get => false;
        }

        public async Task On(IEnumerable<Envelope<IEvent>> events)
        {
            var tags = new Dictionary<DomainId, Dictionary<string, int>>();

            foreach (var @event in events)
            {
                await TrackUsageAsync(@event);

                AddTags(@event, tags);
            }

            foreach (var (appId, updates) in tags)
            {
                await tagService.UpdateAsync(appId, TagGroups.Assets, updates);
            }
        }

        private static void AddTags(Envelope<IEvent> @event, Dictionary<DomainId, Dictionary<string, int>> tags)
        {
            if (@event.Headers.Restored())
            {
                return;
            }

            void AddTags(DomainId appId, HashSet<string>? tagIds, int count)
            {
                if (tagIds != null)
                {
                    foreach (var tag in tagIds)
                    {
                        var perApp = tags.GetOrAddNew(appId);

                        perApp[tag] = perApp.GetOrAddDefault(tag) + count;
                    }
                }
            }

            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    AddTags(assetCreated.AppId.Id, assetCreated.Tags, 1);
                    break;

                case AssetAnnotated assetAnnotated when assetAnnotated.Tags != null && assetAnnotated.OldTags != null:
                    AddTags(assetAnnotated.AppId.Id, assetAnnotated.Tags, 1);
                    AddTags(assetAnnotated.AppId.Id, assetAnnotated.OldTags, -1);
                    break;

                case AssetDeleted assetDeleted:
                    AddTags(assetDeleted.AppId.Id, assetDeleted.OldTags, -1);
                    break;
            }
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
