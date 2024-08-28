// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas;

public sealed class MongoSchemaRepository : MongoSnapshotStoreBase<Schema, MongoSchemaEntity>, ISchemaRepository, IDeleter
{
    public MongoSchemaRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "States_Schemas";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoSchemaEntity>(
                Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IndexedName))
        ], ct);
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
    }

    public async Task<List<Schema>> QueryAllAsync(DomainId appId, CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSchemaRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedAppId == appId && !x.IndexedDeleted)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }

    public async Task<Schema?> FindAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSchemaRepository/FindAsync"))
        {
            var entity =
                await Collection.Find(x => x.IndexedAppId == appId && x.IndexedId == id && !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<Schema?> FindAsync(DomainId appId, string name,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSchemaRepository/FindAsyncByName"))
        {
            var entity =
                await Collection.Find(x => x.IndexedAppId == appId && x.IndexedName == name && !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }
}
