// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Teams;

public sealed class MongoTeamRepository : MongoSnapshotStoreBase<Team, MongoTeamEntity>, ITeamRepository
{
    public MongoTeamRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "States_Teams";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoTeamEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoTeamEntity>(
                Index
                    .Ascending(x => x.IndexedUserIds))
        ], ct);
    }

    public async Task<List<Team>> QueryAllAsync(string contributorId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoTeamRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedUserIds.Contains(contributorId) && !x.IndexedDeleted)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }

    public async Task<Team?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoTeamRepository/FindAsync"))
        {
            var entity =
                await Collection.Find(x => x.DocumentId == id && !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<Team?> FindByAuthDomainAsync(string authDomain,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoTeamRepository/FindByAuthDomainAsync"))
        {
            var entity =
                await Collection.Find(x => x.IndexedAuthDomain == authDomain && !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }
}
