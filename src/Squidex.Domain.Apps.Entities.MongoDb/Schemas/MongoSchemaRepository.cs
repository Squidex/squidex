// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed partial class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository
    {
        public MongoSchemaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_Schemas";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Ascending(x => x.IsDeleted));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Ascending(x => x.Name).Ascending(x => x.IsDeleted));
        }

        public async Task<Guid> FindSchemaIdAsync(Guid appId, string name)
        {
            var schemaEntity =
                await Collection.Find(x => x.AppId == appId && x.Name == name && !x.IsDeleted).Only(x => x.Id).SortByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

            return schemaEntity != null ? Guid.Parse(schemaEntity["_id"].AsString) : Guid.Empty;
        }

        public async Task<IReadOnlyList<Guid>> QuerySchemaIdsAsync(Guid appId)
        {
            var schemaEntities =
                await Collection.Find(x => x.AppId == appId && !x.IsDeleted).Only(x => x.Id)
                    .ToListAsync();

            return schemaEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }
    }
}
