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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class EFSchemaRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFSnapshotStore<TContext, Schema, EFSchemaEntity>(dbContextFactory), ISchemaRepository, ISchemasHash, IDeleter where TContext : DbContext
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFSchemaEntity>().Where(x => x.IndexedAppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFSchemaEntity>().Where(x => x.IndexedId == schema.Id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<List<Schema>> QueryAllAsync(DomainId appId, CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchemaRepository/QueryAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entities =
                await dbContext.Set<EFSchemaEntity>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => !x.IndexedDeleted)
                    .OrderBy(x => x.IndexedName)
                    .ToListAsync(ct);

            return entities.Select(x => x.Document).ToList();
        }
    }

    public async Task<Schema?> FindAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchemaRepository/FindAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFSchemaEntity>()
                    .Where(x => x.IndexedAppId == appId && x.IndexedId == id)
                    .Where(x => !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<Schema?> FindAsync(DomainId appId, string name,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchemaRepository/FindAsyncByName"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity =
                await dbContext.Set<EFSchemaEntity>()
                    .Where(x => x.IndexedAppId == appId && x.IndexedName == name)
                    .Where(x => !x.IndexedDeleted)
                    .FirstOrDefaultAsync(ct);

            return entity?.Document;
        }
    }

    public async Task<SchemasHashKey> GetCurrentHashAsync(App app,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSchemaRepository/GetCurrentHashAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entities =
                await dbContext.Set<EFSchemaEntity>()
                    .Where(x => x.IndexedAppId == app.Id)
                    .Where(x => !x.IndexedDeleted)
                    .Select(x => new { Id = x.IndexedId, x.Version })
                    .ToListAsync(ct);

            return SchemasHashKey.Create(app, entities.ToDictionary(x => x.Id, x => x.Version));
        }
    }

    protected override Expression<Func<SetPropertyCalls<EFSchemaEntity>, SetPropertyCalls<EFSchemaEntity>>> BuildUpdate(EFSchemaEntity entity)
    {
        return u => u
            .SetProperty(x => x.Document, entity.Document)
            .SetProperty(x => x.IndexedAppId, entity.IndexedAppId)
            .SetProperty(x => x.IndexedDeleted, entity.IndexedDeleted)
            .SetProperty(x => x.IndexedId, entity.IndexedId)
            .SetProperty(x => x.IndexedName, entity.IndexedName)
            .SetProperty(x => x.Version, entity.Version);
    }
}
