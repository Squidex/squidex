// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Data;
using System.Runtime.CompilerServices;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class EFRuleEventRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IRuleEventRepository, IDeleter where TContext : DbContext
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>().Where(x => x.AppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    public async IAsyncEnumerable<IRuleEventEntity> QueryPendingAsync(Instant now,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var ruleEvents =
            dbContext.Set<EFRuleEventEntity>().Where(x => x.NextAttempt < now)
                .ToAsyncEnumerable();

        await foreach (var ruleEvent in ruleEvents.WithCancellation(ct))
        {
            yield return ruleEvent;
        }
    }

    public async Task<IResultList<IRuleEventEntity>> QueryByAppAsync(DomainId appId, DomainId? ruleId = null, int skip = 0, int take = 20,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var query =
            dbContext.Set<EFRuleEventEntity>()
                .Where(x => x.AppId == appId)
                .WhereIf(x => x.RuleId == ruleId!.Value, ruleId.HasValue);

        var ruleEventEntities = await query.Skip(skip).Take(take).OrderByDescending(x => x.Created).ToListAsync(ct);
        var ruleEventTotal = (long)ruleEventEntities.Count;

        if (ruleEventTotal >= take || skip > 0)
        {
            ruleEventTotal = await query.CountAsync(ct);
        }

        return ResultList.Create(ruleEventTotal, ruleEventEntities);
    }

    public async Task<IRuleEventEntity?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var ruleEvent =
            await dbContext.Set<EFRuleEventEntity>().Where(x => x.Id == id)
                .FirstOrDefaultAsync(ct);

        return ruleEvent;
    }

    public async Task EnqueueAsync(DomainId id, Instant nextAttempt,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.NextAttempt, nextAttempt),
                ct);
    }

    public async Task CancelByEventAsync(DomainId id,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.NextAttempt, (Instant?)null)
                .SetProperty(x => x.JobResult, RuleJobResult.Cancelled),
                ct);
    }

    public async Task CancelByRuleAsync(DomainId ruleId,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>()
            .Where(x => x.RuleId == ruleId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.NextAttempt, (Instant?)null)
                .SetProperty(x => x.JobResult, RuleJobResult.Cancelled),
                ct);
    }

    public async Task CancelByAppAsync(DomainId appId,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>()
            .Where(x => x.AppId == appId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.NextAttempt, (Instant?)null)
                .SetProperty(x => x.JobResult, RuleJobResult.Cancelled),
                ct);
    }

    public async Task UpdateAsync(RuleJob job, RuleJobUpdate update,
        CancellationToken ct = default)
    {
        Guard.NotNull(job);
        Guard.NotNull(update);

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEventEntity>()
            .Where(x => x.Id == job.Id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Result, update.ExecutionResult)
                .SetProperty(x => x.LastDump, update.ExecutionDump)
                .SetProperty(x => x.JobResult, update.JobResult)
                .SetProperty(x => x.NextAttempt, update.JobNext)
                .SetProperty(x => x.NumCalls, x => x.NumCalls + 1),
                ct);
    }

    public async Task EnqueueAsync(List<RuleEventWrite> jobs,
        CancellationToken ct = default)
    {
        var entities = jobs.Select(EFRuleEventEntity.FromJob).ToList();
        if (entities.Count == 0)
        {
            return;
        }

        await using var dbContext = await CreateDbContextAsync(ct);
        await dbContext.BulkInsertAsync(entities, cancellationToken: ct);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
