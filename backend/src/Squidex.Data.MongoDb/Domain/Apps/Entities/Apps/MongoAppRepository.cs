// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class MongoAppRepository(IMongoDatabase database) : MongoSnapshotStoreBase<App, MongoAppEntity>(database), IAppRepository, IDeleter
{
    protected override string CollectionName()
    {
        return "States_Apps";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedName)),
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedUserIds)),
            new CreateIndexModel<MongoAppEntity>(
                Index
                    .Ascending(x => x.IndexedTeamId)),
        ], ct);
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.DocumentId, app.Id), ct);
    }

    public async Task<List<App>> QueryAllAsync(string contributorId, IEnumerable<string> names,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => (x.IndexedUserIds.Contains(contributorId) || names.Contains(x.IndexedName)) && !x.IndexedDeleted)
                    .ToListAsync(ct);

            return RemoveDuplicateNames(entities);
        }
    }

    public async Task<List<App>> QueryAllAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAllAsync"))
        {
            var entities =
                await Collection.Find(x => x.IndexedTeamId == teamId && !x.IndexedDeleted).SortBy(x => x.IndexedCreated)
                    .ToListAsync(ct);

            return RemoveDuplicateNames(entities);
        }
    }

    public async Task<IResultList<App>> QueryAsync(string? query, Paging paging,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAsync"))
        {
            var filters = new List<FilterDefinition<MongoAppEntity>>
            {
                Filter.Eq(x => x.IndexedDeleted, false),
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                // App names are always lowercase, therefore we can avoid the slow case insensitive search.
                filters.Add(Filter.Regex(x => x.IndexedName, new BsonRegularExpression(Regex.Escape(query.Trim().ToLowerInvariant()))));
            }

            var filter = Filter.And(filters);

            var entities =
                await Collection.Find(filter).SortBy(x => x.IndexedName).Skip(paging.Skip).Limit(paging.Take)
                    .ToListAsync(ct);

            var total = (long)entities.Count;

            if (entities.Count >= paging.Take || paging.Skip > 0)
            {
                total = await Collection.CountDocumentsAsync(filter, cancellationToken: ct);
            }

            return ResultList.Create(total, entities.Select(x => x.Document));
        }
    }

    public async Task<App?> FindAsync(DomainId id,
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

    public async Task<App?> FindAsync(string name,
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

    private static List<App> RemoveDuplicateNames(List<MongoAppEntity> entities)
    {
        var byName = new Dictionary<string, App>();

        // Remove duplicate names, the latest wins.
        foreach (var entity in entities.OrderBy(x => x.IndexedCreated))
        {
            byName[entity.IndexedName] = entity.Document;
        }

        return byName.Values.ToList();
    }
}
