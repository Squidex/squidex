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

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext>
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
                await QueryAsync<EFContentCompleteEntity, EFReferenceCompleteEntity>(app.Id, schemaIds, isSingle, q,
                    ContentQueryBuilder.CreateComplete(dialect), ct) :
                await QueryAsync<EFContentPublishedEntity, EFReferencePublishedEntity>(app.Id, schemaIds, isSingle, q,
                    ContentQueryBuilder.CreatePublished(dialect), ct);
        }
    }

    private async Task<IResultList<Content>> QueryAsync<T, TReference>(
        DomainId appId,
        List<DomainId> schemaIds,
        bool isSingle,
        Q q,
        SqlQueryBuilder queryBuilder,
        CancellationToken ct = default) where T : EFContentEntity where TReference : EFReferenceEntity
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

            var fromKey = DomainId.Combine(appId, q.Referencing);

            var toIds =
                dbContext.Set<TReference>()
                    .Where(x => x.AppId == appId && x.FromKey == fromKey)
                    .Select(x => x.ToId)
                    .ToList();

            var result =
                await dbContext.Set<T>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => schemaIds.Contains(x.IndexedSchemaId))
                    .Where(x => toIds.Contains(x.Id))
                    .Where(x => !x.IsDeleted)
                    .QueryAsync(q, ct);

            return result;
        }

        if (q.Reference != default && schemaIds.Count > 0)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var fromKeys =
                dbContext.Set<TReference>()
                    .Where(x => x.AppId == appId && x.ToId == q.Reference)
                    .Select(x => x.FromKey);

            var result =
                await dbContext.Set<T>()
                    .Where(x => fromKeys.Contains(x.DocumentId))
                    .Where(x => schemaIds.Contains(x.IndexedSchemaId))
                    .Where(x => !x.IsDeleted)
                    .QueryAsync(q, ct);

            return result;
        }

        if (isSingle)
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            queryBuilder.RawWhere(nameof(EFContentEntity.IndexedAppId), CompareOperator.Equals, appId.ToString());
            queryBuilder.RawWhere(nameof(EFContentEntity.IndexedSchemaId), CompareOperator.Equals, schemaIds.Single().ToString());
            queryBuilder.RawWhere(nameof(EFContentEntity.IsDeleted), CompareOperator.Equals, false);
            queryBuilder.WithFilter(q.Query);
            queryBuilder.WithOrders(q.Query);

            return await dbContext.QueryAsync<T>(queryBuilder, q, ct);
        }

        return ResultList.Empty<Content>();
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            QueryIdsAsync<EFContentCompleteEntity>(app.Id, schema.Id, filterNode,
                ContentQueryBuilder.CreateComplete(dialect), ct) :
            QueryIdsAsync<EFContentPublishedEntity>(app.Id, schema.Id, filterNode,
                ContentQueryBuilder.CreatePublished(dialect), ct);
    }

    private async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync<T>(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode, SqlQueryBuilder queryBuilder,
        CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var (query, parameters) =
            queryBuilder
                .RawWhere(nameof(EFContentEntity.IndexedAppId), CompareOperator.Equals, appId.ToString())
                .RawWhere(nameof(EFContentEntity.IndexedSchemaId), CompareOperator.Equals, schemaId.ToString())
                .RawWhere(nameof(EFContentEntity.IsDeleted), CompareOperator.Equals, false)
                .WithField(nameof(EFContentEntity.IndexedSchemaId))
                .WithField(nameof(EFContentEntity.Id))
                .WithField(nameof(EFContentEntity.Status))
                .WithFilter(filterNode)
                .Compile();

        var entities =
            await dbContext.Set<T>()
                .FromSqlRaw(query, parameters)
                .Select(x => new { SchemaId = x.IndexedSchemaId, x.Id, x.Status })
                .ToListAsync(ct);

        return entities.Select(x => new ContentIdStatus(x.SchemaId, x.Id, x.Status)).ToList();
    }
}
