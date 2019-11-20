// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^asset-"; }
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetCreated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 1);

                case AssetUpdated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 0);

                case AssetItemDeleted e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.DeletedSize, -1);
            }

            return TaskHelper.Done;
        }

        private static DateTime GetDate(Envelope<IEvent> @event)
        {
            return @event.Headers.Timestamp().ToDateTimeUtc().Date;
        }

        private Task UpdateSizeAsync(Guid appId, DateTime date, long size, long count)
        {
            var counters = new Counters
            {
                [CounterTotalSize] = size,
                [CounterTotalCount] = count
            };

            var key = GetKey(appId);

            return Task.WhenAll(
                usageStore.TrackUsagesAsync(new UsageUpdate(date, key, Category, counters)),
                usageStore.TrackUsagesAsync(new UsageUpdate(SummaryDate, key, Category, counters)));
        }

        private static string GetKey(Guid appId)
        {
            return $"{appId}_Assets";
        }
    }
}
