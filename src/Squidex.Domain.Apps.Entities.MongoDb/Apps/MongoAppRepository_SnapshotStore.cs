// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
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

        public Task WriteAsync(Guid key, AppState value, long oldVersion, long newVersion)
        {
            return Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u
                .Set(x => x.Name, value.Name)
                .Set(x => x.State, value)
                .Set(x => x.UserIds, value.Contributors.Keys.ToArray())
                .Set(x => x.IsArchived, value.IsArchived));
        }
    }
}
