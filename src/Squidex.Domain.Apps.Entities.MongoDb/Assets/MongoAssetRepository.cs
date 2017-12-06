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
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository, ISnapshotStore<AssetState>
    {
        public MongoAssetRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.State.AppId)
                    .Ascending(x => x.State.FileName)
                    .Ascending(x => x.State.MimeType)
                    .Descending(x => x.State.LastModified));
        }

        public async Task<(AssetState Value, long Version)> ReadAsync(string key)
        {
            var existing =
                await Collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.State, existing.Version);
            }

            return (null, -1);
        }

        public async Task<IReadOnlyList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0)
        {
            var filter = CreateFilter(appId, mimeTypes, ids, query);

            var assetEntities =
                await Collection.Find(filter).Skip(skip).Limit(take).SortByDescending(x => x.State.LastModified)
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
            var (state, etag) = await ReadAsync(id.ToString());

            return state;
        }

        private static FilterDefinition<MongoAssetEntity> CreateFilter(Guid appId, ICollection<string> mimeTypes, ICollection<Guid> ids, string query)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.State.AppId, appId)
            };

            if (ids != null && ids.Count > 0)
            {
                filters.Add(Filter.In(x => x.Id, ids.Select(x => x.ToString())));
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

            return filter;
        }

        public async Task WriteAsync(string key, AssetState value, long oldVersion, long newVersion)
        {
            try
            {
                await Collection.UpdateOneAsync(x => x.Id == key && x.Version == oldVersion,
                    Update
                        .Set(x => x.State, value)
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == key)
                            .Project<MongoAssetEntity>(Projection.Exclude(x => x.Id)).FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion.Version, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
