// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext>
{
    public IAsyncEnumerable<DomainId> StreamIds(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            StreamIds<EFContentCompleteEntity>(appId, schemaIds, ct) :
            StreamIds<EFContentPublishedEntity>(appId, schemaIds, ct);
    }

    private async IAsyncEnumerable<DomainId> StreamIds<T>(DomainId appId, HashSet<DomainId>? schemaIds,
        [EnumeratorCancellation] CancellationToken ct = default) where T : EFContentEntity
    {
        if (schemaIds is { Count: 0 })
        {
            yield break;
        }

        await using var dbContext = await CreateDbContextAsync(ct);

        var query =
            dbContext.Set<T>()
                .Where(x => x.IndexedAppId == appId)
                .WhereIf(x => schemaIds!.Contains(x.IndexedSchemaId), schemaIds is { Count: > 0 })
                .Select(x => x.Id)
                .ToAsyncEnumerable();

        await foreach (var id in query.WithCancellation(ct))
        {
            yield return id;
        }
    }

    public IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            StreamAll<EFContentCompleteEntity>(appId, schemaIds, ct) :
            StreamAll<EFContentPublishedEntity>(appId, schemaIds, ct);
    }

    private async IAsyncEnumerable<Content> StreamAll<T>(DomainId appId, HashSet<DomainId>? schemaIds,
        [EnumeratorCancellation] CancellationToken ct = default) where T : EFContentEntity
    {
        if (schemaIds is { Count: 0 })
        {
            yield break;
        }

        await using var dbContext = await CreateDbContextAsync(ct);

        var query =
            dbContext.Set<T>()
                .Where(x => x.IndexedAppId == appId)
                .WhereIf(x => schemaIds!.Contains(x.IndexedSchemaId), schemaIds is { Count: > 0 })
                .Select(x => x)
                .ToAsyncEnumerable();

        await foreach (var entity in query.WithCancellation(ct))
        {
            yield return entity;
        }
    }

    public IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId reference, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            StreamReferencing<EFContentCompleteEntity, EFReferenceCompleteEntity>(appId, reference, take, ct) :
            StreamReferencing<EFContentPublishedEntity, EFReferencePublishedEntity>(appId, reference, take, ct);
    }

    private async IAsyncEnumerable<Content> StreamReferencing<T, TReference>(DomainId appId, DomainId reference, int take,
        [EnumeratorCancellation] CancellationToken ct = default) where T : EFContentEntity where TReference : EFReferenceEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var query =
            dbContext.Set<T>()
                .Join(dbContext.Set<TReference>(), t => t.DocumentId, r => r.FromKey, (t, r) => new { T = t, R = r })
                .Where(x => x.R.ToId == reference)
                .Where(x => x.R.AppId == appId)
                .Where(x => x.T.IndexedAppId == appId)
                .Select(x => x.T).Distinct()
                .Take(take)
                .ToAsyncEnumerable();

        await foreach (var entity in query.WithCancellation(ct))
        {
            yield return entity;
        }
    }

    public IAsyncEnumerable<Content> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
        CancellationToken ct = default)
    {
        return scope == SearchScope.All ?
            StreamScheduledWithoutDataAsync<EFContentCompleteEntity>(now, ct) :
            StreamScheduledWithoutDataAsync<EFContentPublishedEntity>(now, ct);
    }

    private async IAsyncEnumerable<Content> StreamScheduledWithoutDataAsync<T>(Instant now,
        [EnumeratorCancellation] CancellationToken ct = default) where T : EFContentEntity
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var query =
            dbContext.Set<T>()
                .Where(x => x.ScheduledAt != null && x.ScheduledAt < now)
                .ToAsyncEnumerable();

        await foreach (var entity in query.WithCancellation(ct))
        {
            yield return entity;
        }
    }
}
