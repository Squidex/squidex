// ==========================================================================
//  MongoAssetStatsRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Assets
{
    public partial class MongoAssetStatsRepository : MongoRepositoryBase<MongoAssetStatsEntity>, IAssetStatsRepository, IEventConsumer
    {
        public MongoAssetStatsRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_AssetStats";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetStatsEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Ascending(x => x.Date)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Descending(x => x.Date)));
        }

        public async Task<IReadOnlyList<IAssetStatsEntity>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate)
        {
            var originalSizesEntities =
                await Collection.Find(x => x.AppId == appId && x.Date >= fromDate && x.Date <= toDate).SortBy(x => x.Date)
                    .ToListAsync();

            var enrichedSizes = new List<MongoAssetStatsEntity>();

            var sizesDictionary = originalSizesEntities.ToDictionary(x => x.Date);

            var previousSize = long.MinValue;
            var previousCount = long.MinValue;

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var size = sizesDictionary.GetOrDefault(date);

                if (size != null)
                {
                    previousSize = size.TotalSize;
                    previousCount = size.TotalCount;
                }
                else
                {
                    if (previousSize < 0)
                    {
                        var firstBeforeRangeEntity =
                            await Collection.Find(x => x.AppId == appId && x.Date < fromDate).SortByDescending(x => x.Date)
                                .FirstOrDefaultAsync();

                        previousSize = firstBeforeRangeEntity?.TotalSize ?? 0L;
                        previousCount = firstBeforeRangeEntity?.TotalCount ?? 0L;
                    }

                    size = new MongoAssetStatsEntity
                    {
                        Date = date,
                        TotalSize = previousSize,
                        TotalCount = previousCount
                    };
                }

                enrichedSizes.Add(size);
            }

            return enrichedSizes;
        }

        public async Task<long> GetTotalSizeAsync(Guid appId)
        {
            var totalSizeEntity =
                await Collection.Find(x => x.AppId == appId).SortByDescending(x => x.Date)
                    .FirstOrDefaultAsync();

            return totalSizeEntity?.TotalSize ?? 0;
        }
    }
}
