// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext> : ISnapshotStore<WriteContent>, IDeleter
{
    async IAsyncEnumerable<SnapshotResult<WriteContent>> ISnapshotStore<WriteContent>.ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities = dbContext.Set<EFContentCompleteEntity>().ToAsyncEnumerable();

        await foreach (var entity in entities.WithCancellation(ct))
        {
            yield return new SnapshotResult<WriteContent>(entity.DocumentId, entity.ToState(), entity.Version);
        }
    }

    async Task<SnapshotResult<WriteContent>> ISnapshotStore<WriteContent>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/ReadAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity = await dbContext.Set<EFContentCompleteEntity>().Where(x => x.DocumentId == key).FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                return new SnapshotResult<WriteContent>(default, null!, EtagVersion.Empty);
            }

            return new SnapshotResult<WriteContent>(entity.DocumentId, entity.ToState(), entity.Version);
        }
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/DeleteAppAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                await dbContext.Set<EFContentCompleteEntity>().Where(x => x.IndexedAppId == app.Id)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFContentPublishedEntity>().Where(x => x.IndexedAppId == app.Id)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferenceCompleteEntity>().Where(x => x.AppId == app.Id)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferencePublishedEntity>().Where(x => x.AppId == app.Id)
                    .ExecuteDeleteAsync(ct);

                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    async Task ISnapshotStore<WriteContent>.ClearAsync(
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/ClearAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                await dbContext.Set<EFContentCompleteEntity>()
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFContentPublishedEntity>()
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferenceCompleteEntity>()
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferencePublishedEntity>()
                    .ExecuteDeleteAsync(ct);

                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    async Task ISnapshotStore<WriteContent>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/RemoveAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                await dbContext.Set<EFContentCompleteEntity>().Where(x => x.DocumentId == key)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFContentPublishedEntity>().Where(x => x.DocumentId == key)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferenceCompleteEntity>().Where(x => x.FromKey == key)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferencePublishedEntity>().Where(x => x.FromKey == key)
                    .ExecuteDeleteAsync(ct);

                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    async Task ISnapshotStore<WriteContent>.WriteAsync(SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        // Some data is corrupt and might throw an exception if we do not ignore it.
        if (!IsValid(job.Value))
        {
            return;
        }

        using (Telemetry.Activities.StartActivity("EFContentRepository/WriteAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var appId = job.Value.AppId.Id;

                await dbContext.Set<EFReferenceCompleteEntity>().Where(x => x.FromKey == job.Key)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferencePublishedEntity>().Where(x => x.FromKey == job.Key)
                    .ExecuteDeleteAsync(ct);

                if (job.Value.ShouldWritePublished)
                {
                    await UpsertVersionedPublishedAsync(dbContext, job, ct);
                }
                else
                {
                    await dbContext.Set<EFContentPublishedEntity>().Where(x => x.DocumentId == job.Key)
                        .ExecuteDeleteAsync(ct);
                }

                await UpsertVersionedCompleteAsync(dbContext, job, ct);
                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    async Task ISnapshotStore<WriteContent>.WriteManyAsync(IEnumerable<SnapshotWriteJob<WriteContent>> jobs,
        CancellationToken ct)
    {
        var validJobs = jobs.Where(x => IsValid(x.Value)).ToList();

        using (Telemetry.Activities.StartActivity("EFContentRepository/WriteManyAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var keys = validJobs.Select(x => x.Key);

                var writesToCompleteContents = new List<EFContentCompleteEntity>();
                var writesToCompleteReferences = new List<EFReferenceCompleteEntity>();
                var writesToPublishedContents = new List<EFContentPublishedEntity>();
                var writesToPublishedReferences = new List<EFReferencePublishedEntity>();

                foreach (var job in validJobs)
                {
                    {
                        var (entity, references) = await EFContentCompleteEntity.CreateAsync(job, appProvider, ct);

                        writesToCompleteContents.Add(entity);
                        writesToCompleteReferences.AddRange(references);
                    }

                    if (job.Value.ShouldWritePublished)
                    {
                        var (entity, references) = await EFContentPublishedEntity.CreateAsync(job, appProvider, ct);

                        writesToPublishedContents.Add(entity);
                        writesToPublishedReferences.AddRange(references);
                    }
                }

                await dbContext.BulkInsertAsync(writesToCompleteContents, cancellationToken: ct);
                await dbContext.BulkInsertAsync(writesToCompleteReferences, cancellationToken: ct);
                await dbContext.BulkInsertAsync(writesToPublishedContents, cancellationToken: ct);
                await dbContext.BulkInsertAsync(writesToPublishedReferences, cancellationToken: ct);

                await dbContext.SaveChangesAsync(ct);
                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    private async Task UpsertVersionedPublishedAsync(TContext dbContext, SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        var (entity, references) = await EFContentPublishedEntity.CreateAsync(job, appProvider, ct);

        await dbContext.AddRangeAsync(references);
        await dbContext.UpsertAsync(entity, job.OldVersion, BuildUpdate<EFContentPublishedEntity>, ct);
    }

    private async Task UpsertVersionedCompleteAsync(TContext dbContext, SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        var (entity, references) = await EFContentCompleteEntity.CreateAsync(job, appProvider, ct);

        await dbContext.AddRangeAsync(references);
        await dbContext.UpsertAsync(entity, job.OldVersion, BuildUpdate<EFContentCompleteEntity>, ct);
    }

    private static Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> BuildUpdate<T>(EFContentEntity entity) where T : EFContentEntity
    {
        return b => b
            .SetProperty(x => x.AppId, entity.AppId)
            .SetProperty(x => x.Created, entity.Created)
            .SetProperty(x => x.CreatedBy, entity.CreatedBy)
            .SetProperty(x => x.Data, entity.Data)
            .SetProperty(x => x.IndexedAppId, entity.IndexedAppId)
            .SetProperty(x => x.IndexedSchemaId, entity.IndexedSchemaId)
            .SetProperty(x => x.IsDeleted, entity.IsDeleted)
            .SetProperty(x => x.LastModified, entity.LastModified)
            .SetProperty(x => x.LastModifiedBy, entity.LastModifiedBy)
            .SetProperty(x => x.NewData, entity.NewData)
            .SetProperty(x => x.NewStatus, entity.NewStatus)
            .SetProperty(x => x.ScheduledAt, entity.ScheduledAt)
            .SetProperty(x => x.ScheduleJob, entity.ScheduleJob)
            .SetProperty(x => x.SchemaId, entity.SchemaId)
            .SetProperty(x => x.Status, entity.Status)
            .SetProperty(x => x.TranslationStatus, entity.TranslationStatus)
            .SetProperty(x => x.Version, entity.Version);
    }

    private static bool IsValid(WriteContent state)
    {
        // Some data is corrupt and might throw an exception during migration if we do not skip them.
        return
            state.AppId != null &&
            state.AppId.Id != DomainId.Empty &&
            state.CurrentVersion != null &&
            state.SchemaId != null &&
            state.SchemaId.Id != DomainId.Empty;
    }
}
