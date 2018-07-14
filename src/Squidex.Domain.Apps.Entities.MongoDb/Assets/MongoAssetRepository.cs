﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Edm;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository, IAssetVerifier
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
                new CreateIndexModel<MongoAssetEntity>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.Name)
                        .Descending(x => x.LastModified)));
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, Guid? folderId = null, string query = null)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByQuery"))
            {
                try
                {
                    var odataQuery = EdmAssetModel.Edm.ParseQuery(query);

                    var filter = FindExtensions.BuildQuery(odataQuery, appId, folderId);

                    var contentCount = Collection.Find(filter).CountDocumentsAsync();
                    var contentItems =
                        Collection.Find(filter)
                            .AssetTake(odataQuery)
                            .AssetSkip(odataQuery)
                            .AssetSort(odataQuery)
                            .ToListAsync();

                    await Task.WhenAll(contentItems, contentCount);

                    return ResultList.Create<IAssetEntity>(contentItems.Result, contentCount.Result);
                }
                catch (NotSupportedException)
                {
                    throw new ValidationException("This odata operation is not supported.");
                }
                catch (NotImplementedException)
                {
                    throw new ValidationException("This odata operation is not supported.");
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

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, IList<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>("QueryAsyncByIds"))
            {
                var find = Collection.Find(x => ids.Contains(x.Id)).SortByDescending(x => x.LastModified);

                var assetItems = find.ToListAsync();
                var assetCount = find.CountDocumentsAsync();

                await Task.WhenAll(assetItems, assetCount);

                var items = ids.Select(x => assetItems.Result.Find(a => a.Id == x)).Where(a => a != null).ToList();

                return ResultList.Create(items.OfType<IAssetEntity>().ToList(), assetCount.Result);
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

        public async Task<bool> FolderExistsAsync(Guid id)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var assetsCount =
                    await Collection.Find(x => x.Id == id && x.IsFolder)
                        .CountDocumentsAsync();

                return assetsCount == 1;
            }
        }
    }
}
