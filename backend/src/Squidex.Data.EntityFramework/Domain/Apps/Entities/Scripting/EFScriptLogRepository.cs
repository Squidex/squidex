// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Scripting;

public sealed class EFScriptLogRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IScriptLogRepository where TContext : DbContext
{
    public async Task InsertManyAsync(IEnumerable<ScriptLogRecord> records,
        CancellationToken ct = default)
    {
        Guard.NotNull(records);

        var entities = records.ToList();
        if (entities.Count == 0)
        {
            return;
        }

        await using var dbContext = await CreateDbContextAsync(ct);
        await dbContext.BulkInsertAsync(entities, ct);
    }

    public async Task TrimAsync(DomainId appId, int maxCount,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        // Find the newest record that exceeds the limit. Everything that is not newer is deleted.
        var boundary =
            await dbContext.Set<ScriptLogRecord>()
                .Where(x => x.AppId == appId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(maxCount)
                .Take(1)
                .Select(x => x.Timestamp)
                .ToListAsync(ct);

        if (boundary.Count == 0)
        {
            return;
        }

        var timestamp = boundary[0];

        await dbContext.Set<ScriptLogRecord>().Where(x => x.AppId == appId && x.Timestamp <= timestamp)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAsync(DomainId appId,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<ScriptLogRecord>().Where(x => x.AppId == appId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteOlderThanAsync(Instant timestamp,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<ScriptLogRecord>().Where(x => x.Timestamp < timestamp)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<ScriptLogRecord>> QueryAsync(DomainId appId, string? name, int skip, int take,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var query = dbContext.Set<ScriptLogRecord>().Where(x => x.AppId == appId);

        if (!string.IsNullOrWhiteSpace(name))
        {
            // The name is a path, therefore the filter matches all logs in the same group.
            // The overload with a comparison type cannot be translated to SQL.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
            query = query.Where(x => x.Name.StartsWith(name));
#pragma warning restore RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
        }

        var result =
            await query
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

        return result;
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
