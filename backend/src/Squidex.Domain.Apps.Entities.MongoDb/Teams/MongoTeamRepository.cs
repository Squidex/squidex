// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.DomainObject;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Teams;

public sealed class MongoTeamRepository : MongoSnapshotStoreBase<TeamDomainObject.State, MongoTeamEntity>, ITeamRepository
{
    public MongoTeamRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoTeamEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoTeamEntity>(
                Index
                    .Ascending(x => x.IndexedUserIds))
        }, ct);
    }

    public async Task<List<ITeamEntity>> QueryAllAsync(string contributorId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoTeamRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedUserIds.Contains(contributorId))
                    .ToListAsync(ct);

            return entities.Select(x => (ITeamEntity)x.Document).ToList();
        }
    }

    public async Task<ITeamEntity?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoTeamRepository/FindAsync"))
        {
            var entity =
                await Collection.Find(x => x.DocumentId == id)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }
}
