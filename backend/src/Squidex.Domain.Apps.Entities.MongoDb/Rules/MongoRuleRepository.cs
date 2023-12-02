﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules;

public sealed class MongoRuleRepository : MongoSnapshotStoreBase<Rule, MongoRuleEntity>, IRuleRepository, IDeleter
{
    public MongoRuleRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoRuleEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoRuleEntity>(
                Index
                    .Ascending(x => x.IndexedAppId))
        }, ct);
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
    }

    public async Task<List<Rule>> QueryAllAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoRuleRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedAppId == appId && !x.IndexedDeleted)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }
}
