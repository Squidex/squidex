// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Squidex.Hosting;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.Caching;

public sealed class EFDistributedCache<TContext>(IDbContextFactory<TContext> dbContextFactory, TimeProvider timeProvider)
    : IDistributedCache, IInitializable where TContext : DbContext
{
#pragma warning disable RECS0108 // Warns about static fields in generic types
    private static readonly TimeSpan CleanupTime = TimeSpan.FromMinutes(10);
#pragma warning restore RECS0108 // Warns about static fields in generic types
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

    public byte[] Get(string key)
    {
        throw new NotSupportedException();
    }

    public void Refresh(string key)
    {
        throw new NotSupportedException();
    }

    public void Remove(string key)
    {
        throw new NotSupportedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        throw new NotSupportedException();
    }

    public Task RefreshAsync(string key,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public async Task CleanupAsync(
        CancellationToken token)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var dbContext = await CreateDbContextAsync(token);

        await dbContext.Set<EFCacheEntity>().Where(x => x.Expires < now)
            .ExecuteDeleteAsync(token);
    }

    public async Task RemoveAsync(string key,
        CancellationToken token = default)
    {
        await using var dbContext = await CreateDbContextAsync(token);

        await dbContext.Set<EFCacheEntity>().Where(x => x.Key == key)
            .ExecuteDeleteAsync(token);
    }

    public async Task<byte[]?> GetAsync(string key,
        CancellationToken token = default)
    {
        await using var dbContext = await CreateDbContextAsync(token);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var entry =
            await dbContext.Set<EFCacheEntity>()
                .Where(x => x.Key == key).FirstOrDefaultAsync(token);

        if (entry != null && entry.Expires > now)
        {
            return entry.Value;
        }

        return null;
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        await using var dbContext = await CreateDbContextAsync(token);

        var expires = timeProvider.GetUtcNow().UtcDateTime;

        if (options.AbsoluteExpiration.HasValue)
        {
            expires = options.AbsoluteExpiration.Value.UtcDateTime;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            expires += options.AbsoluteExpirationRelativeToNow.Value;
        }
        else if (options.SlidingExpiration.HasValue)
        {
            expires += options.SlidingExpiration.Value;
        }
        else
        {
            expires = DateTime.MaxValue;
        }

        var entity = new EFCacheEntity { Key = key, Value = value, Expires = expires };
        try
        {
            await dbContext.Set<EFCacheEntity>().AddAsync(entity, token);
            await dbContext.SaveChangesAsync(token);
        }
        finally
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }

        await dbContext.Set<EFCacheEntity>().Where(x => x.Key == key)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Value, value)
                .SetProperty(x => x.Expires, expires),
                token);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
