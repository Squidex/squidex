// ==========================================================================
//  MongoSchemaRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Name));
        }

        public async Task<IReadOnlyList<Guid>> QueryAllSchemaIdsAsync(Guid appId, string name)
        {
            var schemaEntities =
                await Collection.Find(x => x.AppId == appId && x.Name == name).Only(x => x.Id).SortByDescending(x => x.Version)
                    .ToListAsync();

            return schemaEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<IReadOnlyList<Guid>> QueryAllSchemaIdsAsync(Guid appId)
        {
            var schemaEntities =
                await Collection.Find(x => x.AppId == appId).Only(x => x.Id)
                    .ToListAsync();

            return schemaEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }
    }
}
