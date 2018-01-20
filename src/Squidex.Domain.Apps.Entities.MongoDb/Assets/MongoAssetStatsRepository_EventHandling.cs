// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public partial class MongoAssetStatsRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^asset-"; }
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

        private async Task UpdateSizeAsync(Guid appId, DateTime date, long size, long count)
        {
            var id = $"{appId}_{date:yyyy-MM-dd}";

            var assetStatsEntity =
                await Collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            if (assetStatsEntity == null)
            {
                var lastEntity =
                    await Collection.Find(x => x.AssetId == appId).SortByDescending(x => x.Date)
                        .FirstOrDefaultAsync();

                assetStatsEntity = new MongoAssetStatsEntity
                {
                    Id = id,
                    Date = date,
                    AssetId = appId,
                    TotalSize = lastEntity?.TotalSize ?? 0,
                    TotalCount = lastEntity?.TotalCount ?? 0
                };
            }

            assetStatsEntity.TotalSize += size;
            assetStatsEntity.TotalCount += count;

            await Collection.ReplaceOneAsync(x => x.Id == id, assetStatsEntity, Upsert);
        }
    }
}
