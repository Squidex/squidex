// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules;

public sealed class MongoRuleEventRepository : MongoRepositoryBase<MongoRuleEventEntity>, IRuleEventRepository, IDeleter
{
    private readonly MongoRuleStatisticsCollection statisticsCollection;

    public MongoRuleEventRepository(IMongoDatabase database)
        : base(database)
    {
        statisticsCollection = new MongoRuleStatisticsCollection(database);
    }

    protected override string CollectionName()
    {
        return "RuleEvents";
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoRuleEventEntity> collection,
        CancellationToken ct)
    {
        await statisticsCollection.InitializeAsync(ct);

        await collection.Indexes.CreateManyAsync(new[]
        {
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
        }, ct);
    }

    async Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        await statisticsCollection.DeleteAppAsync(app.Id, ct);

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
            await Collection.Find(x => x.JobId == id)
                .FirstOrDefaultAsync(ct);

        return ruleEvent;
    }

    public Task EnqueueAsync(DomainId id, Instant nextAttempt,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.JobId == id, Update.Set(x => x.NextAttempt, nextAttempt), cancellationToken: ct);
    }

    public async Task EnqueueAsync(RuleJob job, Instant? nextAttempt,
        CancellationToken ct = default)
    {
        var entity = MongoRuleEventEntity.FromJob(job, nextAttempt);

        await Collection.InsertOneIfNotExistsAsync(entity, ct);
    }

    public Task CancelByEventAsync(DomainId id,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.JobId == id,
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

        return Task.WhenAll(
            UpdateStatisticsAsync(job, update, ct),
            UpdateEventAsync(job, update, ct));
    }

    private Task UpdateEventAsync(RuleJob job, RuleJobUpdate update,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.JobId == job.Id,
            Update
                .Set(x => x.Result, update.ExecutionResult)
                .Set(x => x.LastDump, update.ExecutionDump)
                .Set(x => x.JobResult, update.JobResult)
                .Set(x => x.NextAttempt, update.JobNext)
                .Inc(x => x.NumCalls, 1),
            cancellationToken: ct);
    }

    private async Task UpdateStatisticsAsync(RuleJob job, RuleJobUpdate update,
        CancellationToken ct = default)
    {
        if (update.ExecutionResult == RuleResult.Success)
        {
            await statisticsCollection.IncrementSuccessAsync(job.AppId, job.RuleId, update.Finished, ct);
        }
        else
        {
            await statisticsCollection.IncrementFailedAsync(job.AppId, job.RuleId, update.Finished, ct);
        }
    }

    public Task<IReadOnlyList<RuleStatistics>> QueryStatisticsByAppAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return statisticsCollection.QueryByAppAsync(appId, ct);
    }
}
