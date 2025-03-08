// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable RECS0096 // Type parameter is never used

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext, TContentContext>
{
    public Task<IResultList<Content>> QueryAsync(App app, Schema schema, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return QueryAsync(app, [schema], true, q, scope, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return QueryAsync(app, schemas, false, q, scope, ct);
    }

    private async Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, bool isSingle, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/QueryAsync"))
        {
            var schemaIds = schemas.Select(x => x.Id).ToList();

            return scope == SearchScope.All ?
                await QueryAsync<EFContentCompleteEntity, EFReferenceCompleteEntity>(
                    app.Id,
                    schemaIds,
                    isSingle,
                    q,
                    ct) :
                await QueryAsync<EFContentPublishedEntity, EFReferencePublishedEntity>(
                    app.Id,
                    schemaIds,
                    isSingle,
                    q,
                    ct);
        }
    }

    private async Task<IResultList<Content>> QueryAsync<T, TReference>(
        DomainId appId,
        List<DomainId> schemaIds,
        bool isSingle,
        Q q,
        CancellationToken ct = default) where T : EFContentEntity
    {
        if (q.Ids is { Count: > 0 } && schemaIds.Count > 0)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var result =
                await dbContext.Set<T>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => schemaIds.Contains(x.IndexedSchemaId))
                    .Where(x => q.Ids.Contains(x.Id))
                    .Where(x => !x.IsDeleted)
                    .QueryAsync(q, ct);

            return result;
        }

        if (q.ScheduledFrom != null && q.ScheduledTo != null && schemaIds.Count > 0)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var result =
                await dbContext.Set<T>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => schemaIds.Contains(x.IndexedSchemaId))
                    .Where(x => x.ScheduledAt >= q.ScheduledFrom && x.ScheduledAt <= q.ScheduledTo)
                    .Where(x => !x.IsDeleted)
                    .QueryAsync(q, ct);

            return result;
        }

        if (q.Referencing != default && schemaIds.Count > 0)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var queryBuilder =
                dbContext.ContentQuery<T>()
                    .Where(ClrFilter.In(nameof(EFContentEntity.IndexedAppId), appId))
                    .Where(ClrFilter.In(nameof(EFContentEntity.IndexedSchemaId), schemaIds))
                    .WhereQuery(nameof(EFContentEntity.Id), CompareOperator.In, parameters =>
                        dbContext.ContentQuery<TReference>(parameters)
                            .Where(ClrFilter.Eq(nameof(EFContentReferenceEntity.AppId), appId))
                            .Where(ClrFilter.Eq(nameof(EFContentReferenceEntity.FromKey), DomainId.Combine(appId, q.Referencing)))
                            .Select(nameof(EFContentReferenceEntity.ToId))
                    )
                    .WhereNotDeleted(q.Query);

            return await QueryAsync<T>(dbContext, queryBuilder, q, ct);
        }

        if (q.Reference != default && schemaIds.Count > 0)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var queryBuilder =
                dbContext.ContentQuery<T>()
                    .WhereQuery(nameof(EFContentEntity.DocumentId), CompareOperator.In, parameters =>
                        dbContext.ContentQuery<TReference>(parameters)
                            .Where(ClrFilter.Eq(nameof(EFContentReferenceEntity.AppId), appId))
                            .Where(ClrFilter.Eq(nameof(EFContentReferenceEntity.ToId), q.Reference))
                            .Select(nameof(EFContentReferenceEntity.FromKey))
                    )
                    .Where(ClrFilter.In(nameof(EFContentEntity.IndexedSchemaId), schemaIds))
                    .WhereNotDeleted(q.Query);

            if (q.Query.Filter?.HasField("IsDeleted") != true)
            {
                queryBuilder.Where(ClrFilter.Eq(nameof(EFContentEntity.IsDeleted), false));
            }

            return await QueryAsync<T>(dbContext, queryBuilder, q, ct);
        }

        if (isSingle)
        {
            var schemaId = schemaIds.Single();

            DbContext dbContext =
                dedicatedTables ?
                // We are not using a transaction here, therefore we can get the table immediately.
                await dynamicTables.CreateDbContextAsync(appId, schemaId, ct) :
                await CreateDbContextAsync(ct);

            await using (dbContext)
            {
                var queryBuilder =
                    dbContext.ContentQuery<T>()
                        .Where(ClrFilter.Eq(nameof(EFContentEntity.IndexedAppId), appId))
                        .Where(ClrFilter.Eq(nameof(EFContentEntity.IndexedSchemaId), schemaId))
                        .WhereNotDeleted(q.Query);

                return await QueryAsync<T>(dbContext, queryBuilder, q, ct);
            }
        }

        return ResultList.Empty<Content>();
    }

    private static async Task<IResultList<Content>> QueryAsync<T>(DbContext dbContext, SqlQueryBuilder queryBuilder, Q q,
        CancellationToken ct) where T : EFContentEntity
    {
        var result = await dbContext.QueryAsync<T>(queryBuilder, q, ct);
        if (result.Count > 0 && q.Fields is { Count: > 0 })
        {
            foreach (var content in result)
            {
                content.Data.LimitFields(q.Fields);
            }
        }

        return result;
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            QueryIdsAsync<EFContentCompleteEntity>(app.Id, schema.Id, filterNode, ct) :
            QueryIdsAsync<EFContentPublishedEntity>(app.Id, schema.Id, filterNode, ct);
    }

    private async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync<T>(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode,
        CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var (sql, parameters) =
            dbContext.ContentQuery<T>()
                .Where(ClrFilter.Eq(nameof(EFContentEntity.IndexedAppId), appId))
                .Where(ClrFilter.Eq(nameof(EFContentEntity.IndexedSchemaId), schemaId))
                .WhereNotDeleted(filterNode)
                .Where(filterNode)
                .Select(nameof(EFContentEntity.IndexedSchemaId))
                .Select(nameof(EFContentEntity.Id))
                .Select(nameof(EFContentEntity.Status))
                .Compile();

        var entities =
            await dbContext.Set<T>().FromSqlRaw(sql, parameters)
                .Select(x => new { SchemaId = x.IndexedSchemaId, x.Id, x.Status }).ToListAsync(ct);

        return entities.Select(x => new ContentIdStatus(x.SchemaId, x.Id, x.Status)).ToList();
    }
}
