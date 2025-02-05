// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class EFRuleRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFSnapshotStore<TContext, Rule, EFRuleEntity>(dbContextFactory), IRuleRepository, IDeleter where TContext : DbContext
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRuleEntity>().Where(x => x.IndexedAppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<List<Rule>> QueryAllAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFRuleRepository/QueryAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entities =
                await dbContext.Set<EFRuleEntity>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => !x.IndexedDeleted)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }
}
