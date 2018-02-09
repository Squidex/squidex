// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed partial class MongoSchemaRepository : ISnapshotStore<SchemaState, Guid>
    {
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

        public Task WriteAsync(Guid key, SchemaState value, long oldVersion, long newVersion)
        {
            return Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u
                .Set(x => x.State, value)
                .Set(x => x.AppId, value.AppId.Id)
                .Set(x => x.Name, value.Name)
                .Set(x => x.IsDeleted, value.IsDeleted));
        }
    }
}
