// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository
    {
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
            return "States_Assets2";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Descending(x => x.LastModified)
                        .Ascending(x => x.Id)
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.ParentId)
                        .Ascending(x => x.Tags)),
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.Slug)),
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.FileHash)
                        .Ascending(x => x.FileName)
                        .Ascending(x => x.FileSize)),
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.Id)
                        .Ascending(x => x.IsDeleted))
            }, ct);
        }

        public async IAsyncEnumerable<IAssetEntity> StreamAll(DomainId appId,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var find = Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted);

            using (var cursor = await find.ToCursorAsync(ct))
            {
                while (await cursor.MoveNextAsync(ct))
                {
                    foreach (var entity in cursor.Current)
                    {
                        yield return entity;
                    }
                }
            }
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, Q q,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/QueryAsync"))
            {
                try
                {
                    if (q.Ids != null && q.Ids.Count > 0)
                    {
                        var filter = BuildFilter(appId, q.Ids.ToHashSet());

                        var assetEntities =
                            await Collection.Find(filter).SortByDescending(x => x.LastModified).ThenBy(x => x.Id)
                                .QueryLimit(q.Query)
                                .QuerySkip(q.Query)
                                .ToListAsync(ct);
                        long assetTotal = assetEntities.Count;

                        if (q.NoTotal)
                        {
                            assetTotal = -1;
                        }
                        else if (assetEntities.Count >= q.Query.Take || q.Query.Skip > 0)
                        {
                            assetTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                        }

                        return ResultList.Create(assetTotal, assetEntities.OfType<IAssetEntity>());
                    }
                    else
                    {
                        var query = q.Query.AdjustToModel(appId);

                        var filter = query.BuildFilter(appId, parentId);

                        var assetEntities =
                            await Collection.Find(filter)
                                .QueryLimit(query)
                                .QuerySkip(query)
                                .QuerySort(query)
                                .ToListAsync(ct);
                        long assetTotal = assetEntities.Count;

                        if (q.NoTotal)
                        {
                            assetTotal = -1;
                        }
                        else if (assetEntities.Count >= q.Query.Take || q.Query.Skip > 0)
                        {
                            assetTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                        }

                        return ResultList.Create<IAssetEntity>(assetTotal, assetEntities);
                    }
                }
                catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
                {
                    throw new DomainException(T.Get("common.resultTooLarge"));
                }
            }
        }

        public async Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/QueryIdsAsync"))
            {
                var assetEntities =
                    await Collection.Find(BuildFilter(appId, ids)).Only(x => x.Id)
                        .ToListAsync(ct);

                var field = Field.Of<MongoAssetFolderEntity>(x => nameof(x.Id));

                return assetEntities.Select(x => DomainId.Create(x[field].AsString)).ToList();
            }
        }

        public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/QueryChildIdsAsync"))
            {
                var assetEntities =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.ParentId == parentId).Only(x => x.Id)
                        .ToListAsync(ct);

                var field = Field.Of<MongoAssetFolderEntity>(x => nameof(x.Id));

                return assetEntities.Select(x => DomainId.Create(x[field].AsString)).ToList();
            }
        }

        public async Task<IAssetEntity?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/FindAssetByHashAsync"))
            {
                var assetEntity =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.FileHash == hash && x.FileName == fileName && x.FileSize == fileSize)
                        .FirstOrDefaultAsync(ct);

                return assetEntity;
            }
        }

        public async Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/FindAssetBySlugAsync"))
            {
                var assetEntity =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.Slug == slug)
                        .FirstOrDefaultAsync(ct);

                return assetEntity;
            }
        }

        public async Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/FindAssetAsync"))
            {
                var documentId = DomainId.Combine(appId, id);

                var assetEntity =
                    await Collection.Find(x => x.DocumentId == documentId && !x.IsDeleted)
                        .FirstOrDefaultAsync(ct);

                return assetEntity;
            }
        }

        public async Task<IAssetEntity?> FindAssetAsync(DomainId id,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/FindAssetAsync"))
            {
                var assetEntity =
                    await Collection.Find(x => x.Id == id && !x.IsDeleted)
                        .FirstOrDefaultAsync(ct);

                return assetEntity;
            }
        }

        private static FilterDefinition<MongoAssetEntity> BuildFilter(DomainId appId, HashSet<DomainId> ids)
        {
            var documentIds = ids.Select(x => DomainId.Combine(appId, x));

            return Filter.And(
                Filter.In(x => x.DocumentId, documentIds),
                Filter.Ne(x => x.IsDeleted, true));
        }
    }
}
