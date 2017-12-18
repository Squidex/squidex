// ==========================================================================
//  MongoAppRepository_SnapshotStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed partial class MongoAppRepository : ISnapshotStore<AppState, Guid>
    {
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
