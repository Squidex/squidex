// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Squidex.Infrastructure.States;

public class EFSnapshotStore<TContext, T, TState>(IDbContextFactory<TContext> dbContextFactory) : ISnapshotStore<T>
    where TContext : DbContext
    where TState : EFState<T>, new()
{
    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/ClearAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<TState>()
                .ExecuteDeleteAsync(ct);
        }
    }

    public async IAsyncEnumerable<SnapshotResult<T>> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/ReadAllAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            await foreach (var entity in dbContext.Set<TState>().ToAsyncEnumerable().WithCancellation(ct))
            {
                if (entity.Document is IOnRead onRead)
                {
                    await onRead.OnReadAsync();
                }

                yield return new SnapshotResult<T>(entity.DocumentId, entity.Document, entity.Version);
            }
        }
    }

    public async Task<SnapshotResult<T>> ReadAsync(DomainId key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/ReadAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity = await dbContext.Set<TState>().Where(x => x.DocumentId == key).FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                return new SnapshotResult<T>(default, default!, EtagVersion.Empty);
            }

            if (entity.Document is IOnRead onRead)
            {
                await onRead.OnReadAsync();
            }

            return new SnapshotResult<T>(entity.DocumentId, entity.Document, entity.Version);
        }
    }

    public async Task RemoveAsync(DomainId key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/RemoveAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<TState>().Where(x => x.DocumentId == key)
                .ExecuteDeleteAsync(ct);
        }
    }

    public async Task WriteAsync(SnapshotWriteJob<T> job,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/WriteAsync"))
        {
            var entity = CreateEntity(job.Key, job.Value, job.NewVersion);

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.UpsertAsync(entity, job.OldVersion, BuildUpdate, ct);
        }
    }

    public async Task WriteManyAsync(IEnumerable<SnapshotWriteJob<T>> jobs,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFStateStore/WriteAsync"))
        {
            var entities = jobs.Select(x => CreateEntity(x.Key, x.Value, x.NewVersion)).ToList();
            if (entities.Count == 0)
            {
                return;
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.BulkUpsertAsync(entities, ct);
        }
    }

    protected Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }

    protected virtual Expression<Func<SetPropertyCalls<TState>, SetPropertyCalls<TState>>> BuildUpdate(TState entity)
    {
        return u => u
            .SetProperty(x => x.Document, entity.Document)
            .SetProperty(x => x.Version, entity.Version);
    }

    protected static TState CreateEntity(DomainId id, T doc, long version)
    {
        var result = new TState
        {
            Document = doc,
            DocumentId = id,
            Version = version,
        };

        result.Prepare();

        return result;
    }
}
