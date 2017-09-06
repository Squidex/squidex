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
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Assets
{
    public partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository, IEventConsumer
    {
        public MongoAssetRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_Assets";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Ascending(x => x.IsDeleted).Descending(x => x.LastModified).Ascending(x => x.FileName).Ascending(x => x.MimeType));
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, IList<Guid> assetIds)
        {
            var assetEntities =
                await Collection.Find(x => assetIds.Contains(x.Id) && x.AppId == appId).Project<BsonDocument>(Project.Include(x => x.Id))
                    .ToListAsync();

            return assetIds.Except(assetEntities.Select(x => Guid.Parse(x["_id"].AsString))).ToList();
        }

        public async Task<IReadOnlyList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0)
        {
            var filter = CreateFilter(appId, mimeTypes, ids, query);

            var assetEntities =
                await Collection.Find(filter).Skip(skip).Limit(take).SortByDescending(x => x.LastModified)
                    .ToListAsync();

            return assetEntities.OfType<IAssetEntity>().ToList();
        }

        public async Task<long> CountAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null)
        {
            var filter = CreateFilter(appId, mimeTypes, ids, query);

            var assetsCount =
                await Collection.Find(filter)
                    .CountAsync();

            return assetsCount;
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var assetEntity =
                await Collection.Find(s => s.Id == id)
                    .FirstOrDefaultAsync();

            return assetEntity;
        }

        private static FilterDefinition<MongoAssetEntity> CreateFilter(Guid appId, ICollection<string> mimeTypes, ICollection<Guid> ids, string query)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.AppId, appId),
                Filter.Eq(x => x.IsDeleted, false)
            };

            if (ids != null && ids.Count > 0)
            {
                filters.Add(Filter.In(x => x.Id, ids));
            }

            if (mimeTypes != null && mimeTypes.Count > 0)
            {
                filters.Add(Filter.In(x => x.MimeType, mimeTypes));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                filters.Add(Filter.Regex(x => x.FileName, new BsonRegularExpression(query, "i")));
            }

            var filter = Filter.And(filters);

            return filter;
        }
    }
}
