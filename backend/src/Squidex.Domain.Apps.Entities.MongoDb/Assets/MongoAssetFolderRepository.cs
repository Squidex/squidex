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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetFolderRepository : MongoRepositoryBase<MongoAssetFolderEntity>, IAssetFolderRepository
    {
        private static readonly Lazy<string> IdField = new Lazy<string>(GetIdField);

        public MongoAssetFolderRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_AssetFolders";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetFolderEntity> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoAssetFolderEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.ParentId))
            }, ct);
        }

        public async Task<IResultList<IAssetFolderEntity>> QueryAsync(DomainId appId, DomainId parentId)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>("QueryAsyncByQuery"))
            {
                var assetFolderEntities =
                    await Collection
                        .Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.ParentId == parentId).SortBy(x => x.FolderName)
                            .ToListAsync();

                return ResultList.Create<IAssetFolderEntity>(assetFolderEntities.Count, assetFolderEntities);
            }
        }

        public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetFolderEntities =
                    await Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.ParentId == parentId).Only(x => x.Id)
                        .ToListAsync();

                return assetFolderEntities.Select(x => DomainId.Create(x[IdField.Value].AsString)).ToList();
            }
        }

        public async Task<IAssetFolderEntity?> FindAssetFolderAsync(DomainId appId, DomainId id)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var documentId = DomainId.Combine(appId, id).ToString();

                var assetFolderEntity =
                    await Collection.Find(x => x.DocumentId == documentId && !x.IsDeleted)
                        .FirstOrDefaultAsync();

                return assetFolderEntity;
            }
        }

        private static string GetIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoAssetFolderEntity)).GetMemberMap(nameof(MongoAssetFolderEntity.DocumentId)).ElementName;
        }
    }
}
