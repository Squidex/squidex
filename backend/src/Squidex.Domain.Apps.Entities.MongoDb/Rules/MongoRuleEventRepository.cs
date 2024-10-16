// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules;

public sealed class MongoRuleEventRepository : MongoRepositoryBase<MongoRuleEventEntity>, IRuleEventRepository, IDeleter
{
    public MongoRuleEventRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "RuleEvents";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoRuleEventEntity> collection,
        CancellationToken ct)
    {
        await collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoRuleEventEntity>(
                Index.Ascending(x => x.NextAttempt)),

            new CreateIndexModel<MongoRuleEventEntity>(
                Index.Ascending(x => x.AppId).Descending(x => x.Created)),

            new CreateIndexModel<MongoRuleEventEntity>(
                Index
                    .Ascending(x => x.Expires),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero
                })
        ], ct);
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
    }

    public Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback,
        CancellationToken ct = default)
    {
        return Collection.Find(x => x.NextAttempt < now).ForEachAsync(callback, ct);
    }

    public async Task<IResultList<IRuleEventEntity>> QueryByAppAsync(DomainId appId, DomainId? ruleId = null, int skip = 0, int take = 20,
        CancellationToken ct = default)
    {
        var filter = Filter.Eq(x => x.AppId, appId);

        if (ruleId != null && ruleId.Value != DomainId.Empty)
        {
            filter = Filter.And(filter, Filter.Eq(x => x.RuleId, ruleId.Value));
        }

        var ruleEventEntities = await Collection.Find(filter).Skip(skip).Limit(take).SortByDescending(x => x.Created).ToListAsync(ct);
        var ruleEventTotal = (long)ruleEventEntities.Count;

        if (ruleEventTotal >= take || skip > 0)
        {
            ruleEventTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
        }

        return ResultList.Create(ruleEventTotal, ruleEventEntities);
    }

    public async Task<IRuleEventEntity> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        var ruleEvent =
            await Collection.Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct);

        return ruleEvent;
    }

    public Task EnqueueAsync(DomainId id, Instant nextAttempt,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.NextAttempt, nextAttempt), cancellationToken: ct);
    }

    public Task CancelByEventAsync(DomainId id,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.Id == id,
            Update
                .Set(x => x.NextAttempt, null)
                .Set(x => x.JobResult, RuleJobResult.Cancelled),
            cancellationToken: ct);
    }

    public Task CancelByRuleAsync(DomainId ruleId,
        CancellationToken ct = default)
    {
        return Collection.UpdateManyAsync(x => x.RuleId == ruleId,
            Update
                .Set(x => x.NextAttempt, null)
                .Set(x => x.JobResult, RuleJobResult.Cancelled),
            cancellationToken: ct);
    }

    public Task CancelByAppAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return Collection.UpdateManyAsync(x => x.AppId == appId,
            Update
                .Set(x => x.NextAttempt, null)
                .Set(x => x.JobResult, RuleJobResult.Cancelled),
            cancellationToken: ct);
    }

    public Task UpdateAsync(RuleJob job, RuleJobUpdate update,
        CancellationToken ct = default)
    {
        Guard.NotNull(job);
        Guard.NotNull(update);

        return Collection.UpdateOneAsync(x => x.Id == job.Id,
            Update
                .Set(x => x.Result, update.ExecutionResult)
                .Set(x => x.LastDump, update.ExecutionDump)
                .Set(x => x.JobResult, update.JobResult)
                .Set(x => x.NextAttempt, update.JobNext)
                .Inc(x => x.NumCalls, 1),
            cancellationToken: ct);
    }

    public async Task EnqueueAsync(List<RuleEventWrite> jobs,
        CancellationToken ct = default)
    {
        var entities = jobs.Select(MongoRuleEventEntity.FromJob).ToList();

        if (entities.Count > 0)
        {
            await Collection.InsertManyAsync(entities, InsertUnordered, ct);
        }
    }
}
