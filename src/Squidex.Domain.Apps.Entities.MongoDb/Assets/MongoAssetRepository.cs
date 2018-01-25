// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Edm;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : MongoRepositoryBase<MongoAssetEntity>, IAssetRepository
    {
        private readonly EdmModelBuilder modelBuilder;

        public MongoAssetRepository(IMongoDatabase database, EdmModelBuilder modelBuilder)
            : base(database)
        {
            this.modelBuilder = modelBuilder;
        }

        protected override string CollectionName()
        {
            return "States_Assets";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.AppId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.FileName)
                    .Ascending(x => x.MimeType)
                    .Descending(x => x.LastModified));
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, string query = null)
        {
            var parsedQuery = ParseQuery(query);

            var assetEntities =
                await Collection
                    .Find(parsedQuery, appId)
                    .Skip(parsedQuery)
                    .Take(parsedQuery)
                    .SortByDescending(x => x.LastModified)
                    .ToListAsync();

            var assetCount = await Collection.Find(parsedQuery, appId).CountAsync();

            return ResultList.Create(assetEntities.OfType<IAssetEntity>().ToList(), assetCount);
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var assetEntity =
                await Collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            return assetEntity;
        }

        private ODataUriParser ParseQuery(string query)
        {
            try
            {
                var model = modelBuilder.BuildEdmModel(new AssetState());

                return model.ParseQuery(query);
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }
    }
}
