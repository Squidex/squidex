// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps;

public sealed class MongoAppRepository : MongoSnapshotStoreBase<AppDomainObject.State, MongoAppEntity>, IAppRepository, IDeleter
{
    public MongoAppRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedName)),
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedUserIds)),
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedTeamId))
        }, ct);
    }

    Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.DocumentId, app.Id), ct);
    }

    public async Task<List<IAppEntity>> QueryAllAsync(string contributorId, IEnumerable<string> names,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => (x.IndexedUserIds.Contains(contributorId) || names.Contains(x.IndexedName)) && !x.IndexedDeleted)
                    .ToListAsync(ct);

            return RemoveDuplcateNames(entities);
        }
    }

    public async Task<List<IAppEntity>> QueryAllAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedTeamId == teamId).SortBy(x => x.IndexedCreated)
                    .ToListAsync(ct);

            return RemoveDuplcateNames(entities);
        }
    }

    public async Task<IAppEntity?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/FindAsync"))
        {
            var entity =
                await Collection.Find(x => x.DocumentId == id && !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<IAppEntity?> FindAsync(string name,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/FindAsyncByName"))
        {
            var entity =
                await Collection.Find(x => x.IndexedName == name && !x.IndexedDeleted).SortByDescending(x => x.IndexedCreated)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    private static List<IAppEntity> RemoveDuplcateNames(List<MongoAppEntity> entities)
    {
        var byName = new Dictionary<string, IAppEntity>();

        // Remove duplicate names, the latest wins.
        foreach (var entity in entities.OrderBy(x => x.IndexedCreated))
        {
            byName[entity.IndexedName] = entity.Document;
        }

        return byName.Values.ToList();
    }
}
