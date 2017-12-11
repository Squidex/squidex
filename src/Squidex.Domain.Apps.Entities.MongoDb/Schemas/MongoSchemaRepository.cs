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
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, ISnapshotStore<SchemaState, Guid>
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

        public async Task<(SchemaState Value, long Version)> ReadAsync(Guid key)
        {
            var existing =
                await Collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.State, existing.Version);
            }

            return (null, EtagVersion.NotFound);
        }

        public async Task<Guid> FindSchemaIdAsync(Guid appId, string name)
        {
            var schemaEntity =
                await Collection.Find(x => x.Name == name).Only(x => x.Id)
                    .FirstOrDefaultAsync();

            return schemaEntity != null ? Guid.Parse(schemaEntity["_id"].AsString) : Guid.Empty;
        }

        public async Task<IReadOnlyList<Guid>> QuerySchemaIdsAsync(Guid appId)
        {
            var schemaEntities =
                await Collection.Find(x => x.AppId == appId).Only(x => x.Id)
                    .ToListAsync();

            return schemaEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task WriteAsync(Guid key, SchemaState value, long oldVersion, long newVersion)
        {
            try
            {
                await Collection.UpdateOneAsync(x => x.Id == key && x.Version == oldVersion,
                    Update
                        .Set(x => x.State, value)
                        .Set(x => x.AppId, value.AppId)
                        .Set(x => x.Name, value.Name)
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == key).Only(x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion["Version"].AsInt64, oldVersion, ex);
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
