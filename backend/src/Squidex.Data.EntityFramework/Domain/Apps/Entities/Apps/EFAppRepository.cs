// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class EFAppRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFSnapshotStore<TContext, App, EFAppEntity>(dbContextFactory), IAppRepository where TContext : DbContext
{
    public async Task<List<App>> QueryAllAsync(string contributorId, IEnumerable<string> names,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/QueryAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var formattedId = EFAppEntity.FormatUserId(contributorId);
            var entities =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.IndexedUserIds.Contains(formattedId) || names.Contains(x.IndexedName))
                    .Where(x => !x.IndexedDeleted)
                    .ToListAsync(ct);

            return RemoveDuplicateNames(entities);
        }
    }

    public async Task<List<App>> QueryAllAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/QueryAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entities =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.IndexedTeamId == teamId)
                    .Where(x => !x.IndexedDeleted)
                    .ToListAsync(ct);

            return RemoveDuplicateNames(entities);
        }
    }

    public async Task<App?> FindAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/FindAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.DocumentId == id.ToString())
                    .Where(x => !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<App?> FindAsync(string name,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAppRepository/FindAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFAppEntity>()
                    .Where(x => x.IndexedName == name)
                    .Where(x => !x.IndexedDeleted)
                    .OrderByDescending(x => x.IndexedCreated)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    private static List<App> RemoveDuplicateNames(List<EFAppEntity> entities)
    {
        var byName = new Dictionary<string, App>();

        // Remove duplicate names, the latest wins.
        foreach (var entity in entities.OrderBy(x => x.IndexedCreated))
        {
            byName[entity.IndexedName] = entity.Document;
        }

        return byName.Values.ToList();
    }

    protected override Expression<Func<SetPropertyCalls<EFAppEntity>, SetPropertyCalls<EFAppEntity>>> BuildUpdate(EFAppEntity entity)
    {
        return u => u
            .SetProperty(x => x.Document, entity.Document)
            .SetProperty(x => x.IndexedCreated, entity.IndexedCreated)
            .SetProperty(x => x.IndexedDeleted, entity.IndexedDeleted)
            .SetProperty(x => x.IndexedName, entity.IndexedName)
            .SetProperty(x => x.IndexedTeamId, entity.IndexedTeamId)
            .SetProperty(x => x.IndexedUserIds, entity.IndexedUserIds)
            .SetProperty(x => x.Version, entity.Version);
    }
}
