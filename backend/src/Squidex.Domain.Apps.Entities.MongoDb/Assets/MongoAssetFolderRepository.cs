// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IResultList<IAssetFolderEntity>> QueryAsync(Guid appId, Guid parentId)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>("QueryAsyncByQuery"))
            {
                var assetFolders =
                    await Collection
                        .Find(x => x.IndexedAppId == appId && !x.IsDeleted && x.ParentId == parentId).SortBy(x => x.FolderName)
                            .ToListAsync();

                return ResultList.Create<IAssetFolderEntity>(assetFolders.Count, assetFolders);
            }
        }

        public async Task<IAssetFolderEntity?> FindAssetFolderAsync(Guid id)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var assetFolderEntity =
                    await Collection.Find(x => x.Id == id)
                        .FirstOrDefaultAsync();

                if (assetFolderEntity?.IsDeleted == true)
                {
                    return null;
                }

                return assetFolderEntity;
            }
        }
    }
}
