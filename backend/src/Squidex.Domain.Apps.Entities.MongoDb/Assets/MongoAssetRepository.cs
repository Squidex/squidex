﻿// ==========================================================================
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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository
    {
        private static readonly Lazy<string> IdField = new Lazy<string>(GetIdField);

        public MongoAssetRepository(IMongoDatabase database)
            : base(database)
        {
        }

        public IMongoCollection<MongoAssetEntity> GetInternalCollection()
        {
            return Collection;
        }

        protected override string CollectionName()
        {
            return "States_Assets";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.ParentId)
                        .Ascending(x => x.Tags)
                        .Descending(x => x.LastModified)),
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.Slug)),
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.FileHash))
            }, ct);
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, ClrQuery query)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByQuery"))
            {
                try
                {
                    query = query.AdjustToModel();

                    var filter = query.BuildFilter(appId, parentId);

                    var assetCount = Collection.Find(filter).CountDocumentsAsync();
                    var assetItems =
                        Collection.Find(filter)
                            .QueryLimit(query)
                            .QuerySkip(query)
                            .QuerySort(query)
                            .ToListAsync();

                    var (items, total) = await AsyncHelper.WhenAll(assetItems, assetCount);

                    return ResultList.Create<IAssetEntity>(total, items);
                }
                catch (MongoQueryException ex) when (ex.Message.Contains("17406"))
                {
                    throw new DomainException("Result set is too large to be retrieved. Use $take parameter to reduce the number of items.");
                }
            }
        }

        public async Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByIds"))
            {
                var assetEntities =
                    await Collection.Find(BuildFilter(appId, ids)).Only(x => x.DocumentId)
                        .ToListAsync();

                return assetEntities.Select(x => DomainId.Create(x[IdField.Value].AsString)).ToList();
            }
        }

        public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetEntities =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.ParentId == parentId).Only(x => x.DocumentId)
                        .ToListAsync();

                return assetEntities.Select(x => DomainId.Create(x[IdField.Value].AsString)).ToList();
            }
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, HashSet<DomainId> ids)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByIds"))
            {
                var assetEntities =
                    await Collection.Find(BuildFilter(appId, ids)).SortByDescending(x => x.LastModified)
                        .ToListAsync();

                return ResultList.Create(assetEntities.Count, assetEntities.OfType<IAssetEntity>());
            }
        }

        public async Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetEntity =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.Slug == slug)
                        .FirstOrDefaultAsync();

                return assetEntity;
            }
        }

        public async Task<IReadOnlyList<IAssetEntity>> QueryByHashAsync(DomainId appId, string hash)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetEntities =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.FileHash == hash)
                        .ToListAsync();

                return assetEntities.OfType<IAssetEntity>().ToList();
            }
        }

        public async Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var documentId = DomainId.Combine(appId, id).ToString();

                var assetEntity =
                    await Collection.Find(x => x.DocumentId == documentId && !x.IsDeleted)
                        .FirstOrDefaultAsync();

                return assetEntity;
            }
        }

        public async Task<IAssetEntity?> FindAssetAsync(DomainId id)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetEntity =
                    await Collection.Find(x => x.Id == id && !x.IsDeleted)
                        .FirstOrDefaultAsync();

                return assetEntity;
            }
        }

        private static FilterDefinition<MongoAssetEntity> BuildFilter(DomainId appId, HashSet<DomainId> ids)
        {
            var documentIds = ids.Select(x => DomainId.Combine(appId, x));

            return Filter.And(
                Filter.In(x => x.Id, documentIds),
                Filter.Ne(x => x.IsDeleted, true));
        }

        private static string GetIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoAssetEntity)).GetMemberMap(nameof(MongoAssetEntity.Id)).ElementName;
        }
    }
}
