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
            return Task.CompletedTask;
        }

        public Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetCreated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 1);

                case AssetUpdated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 0);

                case AssetDeleted e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.DeletedSize, -1);
            }

            return Task.CompletedTask;
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

            var appKey = GetKey(appId);

            return Task.WhenAll(
                usageTracker.TrackAsync(date, appKey, null, counters),
                usageTracker.TrackAsync(SummaryDate, appKey, null, counters));
        }

        private static string GetKey(Guid appId)
        {
            return $"{appId}_Assets";
        }
    }
}
