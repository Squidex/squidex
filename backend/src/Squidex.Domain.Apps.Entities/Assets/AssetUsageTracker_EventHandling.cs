// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.UsageTracking;

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

        public Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetCreated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 1);

                case AssetUpdated e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), e.FileSize, 0);

                case AssetDeleted e:
                    return UpdateSizeAsync(e.AppId.Id, GetDate(@event), -e.DeletedSize, -1);
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
