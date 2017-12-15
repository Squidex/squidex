// ==========================================================================
//  MongoAppRepository.cs
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
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository, ISnapshotStore<AppState, Guid>
    {
        public MongoAppRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_Apps";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.UserIds));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Name));
        }

        public async Task<Guid> FindAppIdByNameAsync(string name)
        {
            var appEntity =
                await Collection.Find(x => x.Name == name).Only(x => x.Id)
                    .FirstOrDefaultAsync();

            return appEntity != null ? Guid.Parse(appEntity["_id"].AsString) : Guid.Empty;
        }

        public async Task<IReadOnlyList<Guid>> QueryUserAppIdsAsync(string userId)
        {
            var appEntities =
                await Collection.Find(x => x.UserIds.Contains(userId)).Only(x => x.Id)
                    .ToListAsync();

            return appEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<(AppState Value, long Version)> ReadAsync(Guid key)
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

        public async Task WriteAsync(Guid key, AppState value, long oldVersion, long newVersion)
        {
            try
            {
                await Collection.UpdateOneAsync(x => x.Id == key && x.Version == oldVersion,
                    Update
                        .Set(x => x.UserIds, value.Contributors.Keys.ToArray())
                        .Set(x => x.Name, value.Name)
                        .Set(x => x.State, value)
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == key)
                            .Project<MongoAppEntity>(Projection.Exclude(x => x.Id)).FirstOrDefaultAsync();

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
