// ==========================================================================
//  MongoAssetRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

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

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.State.AppId)
                    .Ascending(x => x.State.IsDeleted)
                    .Ascending(x => x.State.FileName)
                    .Ascending(x => x.State.MimeType)
                    .Descending(x => x.State.LastModified));
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.State.AppId, appId),
                Filter.Eq(x => x.State.IsDeleted, false)
            };

            if (ids != null && ids.Count > 0)
            {
                filters.Add(Filter.In(x => x.Id, ids));
            }

            if (mimeTypes != null && mimeTypes.Count > 0)
            {
                filters.Add(Filter.In(x => x.State.MimeType, mimeTypes));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                filters.Add(Filter.Regex(x => x.State.FileName, new BsonRegularExpression(query, "i")));
            }

            var filter = Filter.And(filters);

            var find = Collection.Find(filter);

            var assetItems = find.Skip(skip).Limit(take).SortByDescending(x => x.State.LastModified).ToListAsync();
            var assetCount = find.CountAsync();

            await Task.WhenAll(assetItems, assetCount);

            return ResultList.Create<IAssetEntity>(assetItems.Result.Select(x => x.State), assetCount.Result);
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var assetEntity =
                await Collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            return assetEntity?.State;
        }
    }
}
