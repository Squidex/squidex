// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext, TContentContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IDbContextNamedFactory<TContentContext> dbContentContextFactory,
    IAppProvider appProvider,
    IOptions<ContentsOptions> options)
    : IContentRepository
    where TContext : DbContext, IDbContextWithDialect where TContentContext : ContentDbContext
{
    private readonly DynamicTables<TContext, TContentContext> dynamicTables = new DynamicTables<TContext, TContentContext>(dbContextFactory, dbContentContextFactory);

    public async Task<Content?> FindContentAsync(App app, Schema schema, DomainId id, SearchScope scope,
        IEnumerable<string>? fields, // TODO: Not used yet
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/FindContentAsync"))
        {
            return scope == SearchScope.All ?
                await FindContentAsync<EFContentCompleteEntity>(app.Id, schema.Id, id, ct) :
                await FindContentAsync<EFContentPublishedEntity>(app.Id, schema.Id, id, ct);
        }
    }

    public async Task<Content?> FindContentAsync<T>(DomainId appId, DomainId schemaId, DomainId id,
        CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entity =
            await dbContext.Set<T>()
                .Where(x => x.DocumentId == DomainId.Combine(appId, id))
                .Where(x => x.IndexedSchemaId == schemaId)
                .FirstOrDefaultAsync(ct);

        return entity;
    }

    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/QueryIdsAsync"))
        {
            return scope == SearchScope.All ?
                await QueryIdsAsync<EFContentCompleteEntity>(app.Id, ids, ct) :
                await QueryIdsAsync<EFContentPublishedEntity>(app.Id, ids, ct);
        }
    }

    private async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync<T>(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities =
            await dbContext.Set<T>()
                .Where(x => x.IndexedAppId == appId)
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { SchemaId = x.IndexedSchemaId, x.Id, x.Status })
                .ToListAsync(ct);

        return entities.Select(x => new ContentIdStatus(x.SchemaId, x.Id, x.Status)).ToList();
    }

    public async Task<bool> HasReferrersAsync(App app, DomainId reference, SearchScope scope,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/HasReferrersAsync"))
        {
            return scope == SearchScope.All ?
                await HasReferrersAsync<EFReferenceCompleteEntity>(app.Id, reference, ct) :
                await HasReferrersAsync<EFReferencePublishedEntity>(app.Id, reference, ct);
        }
    }

    public async Task<bool> HasReferrersAsync<TReference>(DomainId appId, DomainId reference,
        CancellationToken ct = default) where TReference : EFContentReferenceEntity
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/QueryIdsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var result =
                await dbContext.Set<TReference>()
                    .Where(x => x.AppId == appId)
                    .Where(x => x.ToId == reference)
                    .AnyAsync(ct);

            return result;
        }
    }

    public Task ResetScheduledAsync(DomainId appId, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            ResetScheduledAsync<EFContentCompleteEntity>(appId, id, ct) :
            ResetScheduledAsync<EFContentPublishedEntity>(appId, id, ct);
    }

    public async Task ResetScheduledAsync<T>(DomainId appId, DomainId id,
        CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<T>()
            .Where(x => x.DocumentId == DomainId.Combine(appId, id))
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.ScheduledAt, (Instant?)null)
                .SetProperty(x => x.ScheduleJob, (ScheduleJob?)null),
                ct);
    }

    public Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default)
    {
        return Task.FromResult<List<IndexDefinition>>([]);
    }

    public Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    private async Task<TContext> CreateDbContextAsync(
        CancellationToken ct)
    {
        return await dbContextFactory.CreateDbContextAsync(ct);
    }
}
