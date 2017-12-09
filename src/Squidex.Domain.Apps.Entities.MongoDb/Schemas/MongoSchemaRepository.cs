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
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, ISnapshotStore<SchemaState>
    {
        public MongoSchemaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Snapshots_Schemas";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.State.AppId));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.State.Name));
        }

        public async Task<(SchemaState Value, long Version)> ReadAsync(string key)
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

        public async Task<Guid> FindSchemaIdAsync(Guid appId, string name)
        {
            var schemaEntity =
                await Collection.Find(x => x.State.Name == name).Only(x => x.Id)
                    .FirstOrDefaultAsync();

            return schemaEntity != null ? Guid.Parse(schemaEntity.Id) : Guid.Empty;
        }

        public async Task<IReadOnlyList<Guid>> QuerySchemaIdsAsync(Guid appId)
        {
            var schemaEntities =
                await Collection.Find(x => x.State.AppId == appId).Only(x => x.Id)
                    .ToListAsync();

            return schemaEntities.Select(x => Guid.Parse(x.Id)).ToList();
        }

        public async Task WriteAsync(string key, SchemaState value, long oldVersion, long newVersion)
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
                        await Collection.Find(x => x.Id == key).Only(x => x.Version)
                            .FirstOrDefaultAsync();

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
