// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class EFTextIndexerState<TContext>(IDbContextFactory<TContext> dbContextFactory, IContentRepository contentRepository)
    : ITextIndexerState, IDeleter
    where TContext : DbContext, IDbContextWithDialect
{
    int IDeleter.Order => -2000;

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var (query, parameters) =
            dbContext.Query<TextContentState>()
                .Where(ClrFilter.Gt(nameof(TextContentState.UniqueContentId), new UniqueContentId(app.Id, DomainId.Empty).ToParseableString()))
                .OrderAsc(nameof(TextContentState.UniqueContentId))
                .OrderAsc(nameof(TextContentState.State))
                .Compile();

        var ids =
            dbContext.Set<TextContentState>()
                .FromSqlRaw(query, parameters)
                .ToAsyncEnumerable()
                .TakeWhile(x => x.UniqueContentId.AppId == app.Id)
                .Take(int.MaxValue)
                .Select(x => x.UniqueContentId);

        await DeleteInBatchesAsync(ids, ct);
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        var ids =
            contentRepository.StreamIds(app.Id, [schema.Id], SearchScope.All, ct)
                .Select(x => new UniqueContentId(app.Id, x));

        await DeleteInBatchesAsync(ids, ct);
    }

    private async Task DeleteInBatchesAsync(IAsyncEnumerable<UniqueContentId> ids,
        CancellationToken ct)
    {
        var dbContext = await CreateDbContextAsync(ct);
        await foreach (var batch in ids.Batch(1000, ct).WithCancellation(ct))
        {
            await dbContext.Set<TextContentState>().Where(x => batch.Contains(x.UniqueContentId))
                .ExecuteDeleteAsync(ct);
        }
    }

    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<TextContentState>()
            .ExecuteDeleteAsync(ct);
    }

    public async Task<Dictionary<UniqueContentId, TextContentState>> GetAsync(HashSet<UniqueContentId> ids,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities =
            await dbContext.Set<TextContentState>().Where(x => ids.Contains(x.UniqueContentId))
                .ToListAsync(ct);

        return entities.ToDictionary(x => x.UniqueContentId);
    }

    public async Task SetAsync(List<TextContentState> updates,
        CancellationToken ct = default)
    {
        var toDelete = new List<TextContentState>();
        var toUpsert = new List<TextContentState>();

        foreach (var update in updates)
        {
            if (update.State == TextState.Deleted)
            {
                toDelete.Add(update);
            }
            else
            {
                toUpsert.Add(update);
            }
        }

        if (toDelete.Count == 0 && toUpsert.Count == 0)
        {
            return;
        }

        await using var dbContext = await CreateDbContextAsync(ct);
        await dbContext.BulkDeleteAsync(toDelete, cancellationToken: ct);
        await dbContext.BulkInsertOrUpdateAsync(toUpsert, cancellationToken: ct);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
