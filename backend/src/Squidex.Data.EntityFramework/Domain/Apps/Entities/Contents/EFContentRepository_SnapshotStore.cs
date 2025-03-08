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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable RECS0096 // Type parameter is never used

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext, TContentContext> : ISnapshotStore<WriteContent>, IDeleter
{
    private readonly bool dedicatedTables = options.Value.OptimizeForSelfHosting;

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

    public async Task<SnapshotResult<WriteContent>> ReadAsync(DomainId key,
        CancellationToken ct = default)
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
            await DeleteCoreAsync(
                x => x.IndexedAppId == app.Id,
                x => x.IndexedAppId == app.Id,
                x => x.AppId == app.Id,
                x => x.AppId == app.Id,
                x => x.AppId == app.Id,
                ct);
        }
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/DeleteSchemaAsync"))
        {
            await DeleteCoreAsync(
                x => x.IndexedAppId == app.Id && x.IndexedSchemaId == schema.Id,
                x => x.IndexedAppId == app.Id && x.IndexedSchemaId == schema.Id,
                x => x.AppId == app.Id && x.FromSchema == schema.Id,
                x => x.AppId == app.Id && x.FromSchema == schema.Id,
                x => x.AppId == app.Id && x.SchemaId == schema.Id,
                ct);
        }
    }

    async Task ISnapshotStore<WriteContent>.ClearAsync(
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/ClearAsync"))
        {
            await DeleteCoreAsync(
                x => true,
                x => true,
                x => true,
                x => true,
                x => true,
                ct);
        }
    }

    private async Task DeleteCoreAsync(
        Expression<Func<EFContentCompleteEntity, bool>> filterComplete,
        Expression<Func<EFContentPublishedEntity, bool>> filterPublished,
        Expression<Func<EFReferenceCompleteEntity, bool>> filterCompleteReferences,
        Expression<Func<EFReferencePublishedEntity, bool>> filterPublishedReferences,
        Func<DynamicContextName, bool> filterContexts,
        CancellationToken ct)
    {
        List<DynamicContextName>? dynamicTableNames = null;
        if (dedicatedTables)
        {
            dynamicTableNames = await dynamicTables.GetContextNames(ct).Where(filterContexts).ToListAsync(ct);

            // Ensure that the tables are created outside of the transaction.
            foreach (var name in dynamicTableNames)
            {
                await dynamicTables.EnsureDbContextAsync(name);
            }
        }

        await using var dbContext = await CreateDbContextAsync(ct);
        await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await dbContext.Set<EFContentCompleteEntity>().Where(filterComplete)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFContentPublishedEntity>().Where(filterPublished)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFReferenceCompleteEntity>().Where(filterCompleteReferences)
                .ExecuteDeleteAsync(ct);

            await dbContext.Set<EFReferencePublishedEntity>().Where(filterPublishedReferences)
                .ExecuteDeleteAsync(ct);

            // Dynamic tables are only valid if we do use dedicated tables.
            if (dynamicTableNames != null)
            {
                foreach (var contextName in dynamicTableNames)
                {
                    var contentDbContext = await dynamicTables.CreateDbContextAsync(contextName, ct);

                    await contentDbContext.Set<EFContentCompleteEntity>().Where(filterComplete)
                        .ExecuteDeleteAsync(ct);

                    await contentDbContext.Set<EFContentPublishedEntity>().Where(filterPublished)
                        .ExecuteDeleteAsync(ct);
                }
            }

            await dbTransaction.CommitAsync(ct);
        }
        catch
        {
            await dbTransaction.RollbackAsync(ct);
            throw;
        }
    }

    async Task ISnapshotStore<WriteContent>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFContentRepository/RemoveAsync"))
        {
            WriteContent? existing = null;
            if (dedicatedTables)
            {
                var found = await ReadAsync(key, ct);
                if (found.Value == null)
                {
                    return;
                }

                existing = found.Value;

                // Ensure that the tables are created outside of the transaction.
                await dynamicTables.EnsureDbContextAsync(existing.AppId.Id, existing.SchemaId.Id);
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                // The existing entity is only valid if we use dedicated tables.
                if (existing != null)
                {
                    var contentDbContext = await dynamicTables.CreateDbContextAsync(existing.AppId.Id, existing.AppId.Id, ct);

                    await RemoveCoreAsync(contentDbContext, key, ct);
                }

                // Remove this last, because we need to query the collection.
                await RemoveCoreAsync(dbContext, key, ct);

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

    private static async Task RemoveCoreAsync(DbContext dbContext, DomainId key, CancellationToken ct)
    {
        await dbContext.Set<EFContentCompleteEntity>().Where(x => x.DocumentId == key)
            .ExecuteDeleteAsync(ct);

        await dbContext.Set<EFContentPublishedEntity>().Where(x => x.DocumentId == key)
            .ExecuteDeleteAsync(ct);
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
            if (dedicatedTables)
            {
                // Ensure that the tables are created outside of the transaction.
                await dynamicTables.EnsureDbContextAsync(job.Value.AppId.Id, job.Value.SchemaId.Id);
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                await dbContext.Set<EFReferenceCompleteEntity>().Where(x => x.FromKey == job.Key)
                    .ExecuteDeleteAsync(ct);

                await dbContext.Set<EFReferencePublishedEntity>().Where(x => x.FromKey == job.Key)
                    .ExecuteDeleteAsync(ct);

                var (completeEntity, completeReferences) = await EFContentCompleteEntity.CreateAsync(job, appProvider, ct);

                await dbContext.AddRangeAsync(completeReferences);
                await dbContext.UpsertAsync(completeEntity, job.OldVersion, ct);

                EFContentPublishedEntity? publishedEntity = null;
                if (job.Value.ShouldWritePublished())
                {
                    var (entity, publishedReferences) = await EFContentPublishedEntity.CreateAsync(job, appProvider, ct);

                    await dbContext.AddRangeAsync(publishedReferences);
                    await dbContext.UpsertAsync(entity, job.OldVersion, ct);

                    publishedEntity = entity;
                }
                else
                {
                    await dbContext.RemoveAsync<EFContentPublishedEntity>(job.Key, ct);
                }

                if (dedicatedTables)
                {
                    var contentDbContext = await dynamicTables.CreateDbContextAsync(job.Value.AppId.Id, job.Value.AppId.Id, ct);

                    await contentDbContext.UpsertAsync(completeEntity, job.OldVersion, ct);
                    await contentDbContext.UpsertOrDeleteAsync(publishedEntity, job.Key, job.OldVersion, ct);
                }

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
        // Some data is corrupt and might throw an exception if we do not ignore it.
        var validJobs = jobs.Where(x => IsValid(x.Value)).ToList();
        if (validJobs.Count == 0)
        {
            return;
        }

        using (Telemetry.Activities.StartActivity("EFContentRepository/WriteManyAsync"))
        {
            if (dedicatedTables)
            {
                // Ensure that the tables are created outside of the transaction.
                foreach (var (appId, schemaId) in validJobs.Select(x => (AppId: x.Value.AppId.Id, SchemaId: x.Value.SchemaId.Id)).Distinct())
                {
                    await dynamicTables.EnsureDbContextAsync(appId, schemaId);
                }
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
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

                    if (job.Value.ShouldWritePublished())
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

                if (dedicatedTables)
                {
                    foreach (var bySchema in writesToCompleteContents.GroupBy(x => (AppId: x.AppId.Id, SchemaId: x.SchemaId.Id)))
                    {
                        var contentDbContext = await dynamicTables.CreateDbContextAsync(bySchema.Key.AppId, bySchema.Key.SchemaId, ct);

                        // Just fetch the published context, so that we can reuse the context.
                        var publishedContents = writesToPublishedContents.Where(x => x.AppId.Id == bySchema.Key.AppId && x.SchemaId.Id == bySchema.Key.SchemaId);

                        await contentDbContext.BulkInsertAsync(bySchema, cancellationToken: ct);
                        await contentDbContext.BulkInsertAsync(publishedContents, cancellationToken: ct);
                    }
                }

                await dbTransaction.CommitAsync(ct);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }
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
