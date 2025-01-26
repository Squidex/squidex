// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NodaTime;
using Squidex.Hosting;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.Log;

public sealed class EFRequestLogRepository<TContext>(IDbContextFactory<TContext> dbContextFactory, IOptions<RequestLogStoreOptions> options)
    : IRequestLogRepository, IInitializable where TContext : DbContext
{
#pragma warning disable RECS0108 // Warns about static fields in generic types
    private static readonly TimeSpan CleanupTime = TimeSpan.FromMinutes(10);
#pragma warning restore RECS0108 // Warns about static fields in generic types
    private readonly RequestLogStoreOptions options = options.Value;
    private CompletionTimer? timer;

    public Task InitializeAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer(CleanupTime, CleanupAsync);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    private async Task CleanupAsync(
        CancellationToken ct)
    {
        var maxAge = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(options.StoreRetentionInDays));

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRequestEntity>().Where(x => x.Timestamp < maxAge)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFRequestEntity>().Where(x => x.Key == key)
            .ExecuteDeleteAsync(ct);
    }

    public async Task InsertManyAsync(IEnumerable<Request> items,
        CancellationToken ct = default)
    {
        Guard.NotNull(items);

        var entities = items.Select(EFRequestEntity.FromRequest).ToList();
        if (entities.Count == 0)
        {
            return;
        }

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.BulkInsertAsync(entities, cancellationToken: ct);
    }

    public async IAsyncEnumerable<Request> QueryAllAsync(string key, Instant fromTime, Instant toTime,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await CreateDbContextAsync(ct);

        var entities =
            dbContext.Set<EFRequestEntity>()
                .Where(x => x.Key == key)
                .Where(x => x.Timestamp >= fromTime && x.Timestamp <= toTime)
                .ToAsyncEnumerable();

        await foreach (var entity in entities.WithCancellation(ct))
        {
            yield return entity.ToRequest();
        }
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
