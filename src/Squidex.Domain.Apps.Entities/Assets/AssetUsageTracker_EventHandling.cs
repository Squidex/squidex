// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.Dispatching;
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

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(AssetCreated @event, EnvelopeHeaders headers)
        {
            return UpdateSizeAsync(@event.AppId.Id, headers.Timestamp().ToDateTimeUtc().Date, @event.FileSize, 1);
        }

        protected Task On(AssetUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSizeAsync(@event.AppId.Id, headers.Timestamp().ToDateTimeUtc().Date, @event.FileSize, 0);
        }

        protected Task On(AssetDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSizeAsync(@event.AppId.Id, headers.Timestamp().ToDateTimeUtc().Date, -@event.DeletedSize, -1);
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
