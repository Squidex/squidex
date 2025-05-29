// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace Squidex.Infrastructure;

public sealed class PooledDbNamedContextFactory<TContext>(
    IServiceProvider serviceProvider,
    Action<DbContextOptionsBuilder<TContext>, string> configure)
    : IDbContextNamedFactory<TContext>, IAsyncDisposable where TContext : DbContext
{
    private readonly ConcurrentDictionary<string, DbContextPool<TContext>> pools = new ();

    public async ValueTask DisposeAsync()
    {
        foreach (var (_, pool) in pools)
        {
            await pool.DisposeAsync();
        }
    }

    public async Task<TContext> CreateDbContextAsync(string name,
        CancellationToken ct = default)
    {
        var pool = pools.GetOrAdd(name, CreatePool);

        var lease = new DbContextLease(pool, true);
        await lease.Context.SetLeaseAsync(lease, ct);

        return (TContext)lease.Context;
    }

    private DbContextPool<TContext> CreatePool(string name)
    {
        var builder =
            new DbContextOptionsBuilder<TContext>()
                .UsePoolSize(128)
                .UsePrefix(name);

        configure(builder, name);

        return new DbContextPool<TContext>(builder.Options, serviceProvider);
    }
}
