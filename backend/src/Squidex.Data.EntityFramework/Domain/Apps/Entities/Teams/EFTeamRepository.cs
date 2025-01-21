// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Teams;

public sealed class EFTeamRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFSnapshotStore<TContext, Team, EFTeamEntity>(dbContextFactory), ITeamRepository where TContext : DbContext
{
    public async Task<List<Team>> QueryAllAsync(string contributorId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTeamRepository/QueryAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var formattedId = EFTeamEntity.FormatUserId(contributorId);
            var entities =
                await dbContext.Set<EFTeamEntity>()
                    .Where(x => x.IndexedUserIds.Contains(formattedId))
                    .Where(x => !x.IndexedDeleted)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }

    public async Task<Team?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTeamRepository/FindAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFTeamEntity>()
                    .Where(x => x.DocumentId == id.ToString())
                    .Where(x => !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<Team?> FindByAuthDomainAsync(string authDomain,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFTeamRepository/FindByAuthDomainAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFTeamEntity>()
                    .Where(x => x.IndexedAuthDomain == authDomain)
                    .Where(x => !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }
}
