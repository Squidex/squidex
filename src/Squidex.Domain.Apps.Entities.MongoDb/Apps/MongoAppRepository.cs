// ==========================================================================
//  MongoAppRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository, ISnapshotStore<AppState>
    {
        public MongoAppRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.UserIds));
        }

        public async Task<(AppState Value, long Version)> ReadAsync(string key)
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

        public async Task<IReadOnlyList<string>> QueryUserAppNamesAsync(string userId)
        {
            var appEntities =
                await Collection.Find(x => x.UserIds.Contains(userId)).Project<MongoAppEntity>(Projection.Include(x => x.Id)).ToListAsync();

            return appEntities.Select(x => x.Id).ToList();
        }

        public async Task WriteAsync(string key, AppState value, long oldVersion, long newVersion)
        {
            try
            {
                await Collection.UpdateOneAsync(x => x.Id == key && x.Version == oldVersion,
                    Update
                        .Set(x => x.UserIds, value.Contributors.Keys.ToArray())
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
