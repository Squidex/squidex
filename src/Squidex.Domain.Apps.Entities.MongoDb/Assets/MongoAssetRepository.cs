// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository
    {
        public MongoAssetRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_Assets";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection, CancellationToken ct = default(CancellationToken))
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.FileName)
                        .Ascending(x => x.Tags)
                        .Descending(x => x.LastModified)),
                cancellationToken: ct);
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, Query query)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByQuery"))
            {
                try
                {
                    query = query.AdjustToModel();

                    var filter = query.BuildFilter(appId);

                    var contentCount = Collection.Find(filter).CountDocumentsAsync();
                    var contentItems =
                        Collection.Find(filter)
                            .AssetTake(query)
                            .AssetSkip(query)
                            .AssetSort(query)
                            .ToListAsync();

                    await Task.WhenAll(contentItems, contentCount);

                    return ResultList.Create<IAssetEntity>(contentCount.Result, contentItems.Result);
                }
                catch (MongoQueryException ex)
                {
                    if (ex.Message.Contains("17406"))
                    {
                        throw new DomainException("Result set is too large to be retrieved. Use $top parameter to reduce the number of items.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, HashSet<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByIds"))
            {
                var find = Collection.Find(x => ids.Contains(x.Id)).SortByDescending(x => x.LastModified);

                var assetItems = find.ToListAsync();
                var assetCount = find.CountDocumentsAsync();

                await Task.WhenAll(assetItems, assetCount);

                return ResultList.Create(assetCount.Result, assetItems.Result.OfType<IAssetEntity>());
            }
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetEntity =
                    await Collection.Find(x => x.Id == id)
                        .FirstOrDefaultAsync();

                return assetEntity;
            }
        }

        public Task RemoveAsync(Guid appId)
        {
            return Collection.DeleteManyAsync(x => x.IndexedAppId == appId);
        }
    }
}
