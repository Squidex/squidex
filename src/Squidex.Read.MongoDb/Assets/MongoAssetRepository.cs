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
using Squidex.Core.Schemas;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Assets;
using Squidex.Read.Assets.Repositories;

namespace Squidex.Read.MongoDb.Assets
{
    public partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository, IAssetTester, IEventConsumer
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
            return collection.Indexes.CreateOneAsync(IndexKeys.Descending(x => x.LastModified).Ascending(x => x.AppId).Ascending(x => x.FileName).Ascending(x => x.MimeType));
        }

        public async Task<bool> IsValidAsync(Guid assetId)
        {
            return await Collection.Find(x => x.Id == assetId).CountAsync() == 1;
        }

        public async Task<IReadOnlyList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0)
        {
            var filter = CreateFilter(appId, mimeTypes, ids, query);

            var assets =
                await Collection.Find(filter).Skip(skip).Limit(take).SortByDescending(x => x.LastModified).ToListAsync();

            return assets.OfType<IAssetEntity>().ToList();
        }

        public async Task<long> CountAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null)
        {
            var filter = CreateFilter(appId, mimeTypes, ids, query);

            var count =
                await Collection.Find(filter).CountAsync();

            return count;
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var entity =
                await Collection.Find(s => s.Id == id).FirstOrDefaultAsync();

            return entity;
        }

        private static FilterDefinition<MongoAssetEntity> CreateFilter(Guid appId, ICollection<string> mimeTypes, ICollection<Guid> ids, string query)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.AppId, appId)
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
