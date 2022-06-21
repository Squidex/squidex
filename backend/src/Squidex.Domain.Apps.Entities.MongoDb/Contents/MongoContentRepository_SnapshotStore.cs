// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentDomainObject.State>, IDeleter
    {
        IAsyncEnumerable<SnapshotResult<ContentDomainObject.State>> ISnapshotStore<ContentDomainObject.State>.ReadAllAsync(
            CancellationToken ct)
        {
            return collectionAll.StreamAll(ct)
                .Select(x => new SnapshotResult<ContentDomainObject.State>(x.DocumentId, x.ToState(), x.Version, true));
        }

        async Task<SnapshotResult<ContentDomainObject.State>> ISnapshotStore<ContentDomainObject.State>.ReadAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/ReadAsync"))
            {
                var existing =
                    await collectionAll.FindAsync(key, ct);

                if (existing?.IsSnapshot == true)
                {
                    return new SnapshotResult<ContentDomainObject.State>(existing.DocumentId, existing.ToState(), existing.Version);
                }

                return new SnapshotResult<ContentDomainObject.State>(default, null!, EtagVersion.Empty);
            }
        }

        async Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/DeleteAppAsync"))
            {
                await collectionAll.DeleteAppAsync(app.Id, ct);
                await collectionPublished.DeleteAppAsync(app.Id, ct);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.ClearAsync(
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/ClearAsync"))
            {
                await collectionAll.ClearAsync(ct);
                await collectionPublished.ClearAsync(ct);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.RemoveAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/RemoveAsync"))
            {
                await collectionAll.RemoveAsync(key, ct);
                await collectionPublished.RemoveAsync(key, ct);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.WriteAsync(SnapshotWriteJob<ContentDomainObject.State> job,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteAsync"))
            {
                if (job.Value.SchemaId.Id == DomainId.Empty)
                {
                    return;
                }

                await Task.WhenAll(
                    UpsertDraftContentAsync(job, ct),
                    UpsertOrDeletePublishedAsync(job, ct));
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.WriteManyAsync(IEnumerable<SnapshotWriteJob<ContentDomainObject.State>> jobs,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteManyAsync"))
            {
                var entitiesPublished = new List<(MongoContentEntity, PersistenceAction)>();
                var entitiesAll = new List<(MongoContentEntity, PersistenceAction)>();

                foreach (var job in jobs.Where(IsValid))
                {
                    if (ShouldWritePublished(job.Value))
                    {
                        entitiesPublished.Add((await MongoContentEntity.CreatePublishedAsync(job, appProvider), job.Action));
                    }

                    entitiesAll.Add((await MongoContentEntity.CreateDraftAsync(job, appProvider), job.Action));
                }

                await Task.WhenAll(
                    collectionPublished.InsertManyAsync(entitiesPublished, ct),
                    collectionAll.InsertManyAsync(entitiesAll, ct));
            }
        }

        private async Task UpsertOrDeletePublishedAsync(SnapshotWriteJob<ContentDomainObject.State> job,
            CancellationToken ct = default)
        {
            if (ShouldWritePublished(job.Value))
            {
                await UpsertPublishedContentAsync(job, ct);
            }
            else
            {
                await DeletePublishedContentAsync(job.Value.AppId.Id, job.Value.Id, ct);
            }
        }

        private Task DeletePublishedContentAsync(DomainId appId, DomainId id,
            CancellationToken ct = default)
        {
            var documentId = DomainId.Combine(appId, id);

            return collectionPublished.RemoveAsync(documentId, ct);
        }

        private async Task UpsertDraftContentAsync(SnapshotWriteJob<ContentDomainObject.State> job,
            CancellationToken ct = default)
        {
            var entity = await MongoContentEntity.CreateDraftAsync(job, appProvider);

            await collectionAll.UpsertVersionedAsync(entity.DocumentId, job.OldVersion, entity, job.Action, ct);
        }

        private async Task UpsertPublishedContentAsync(SnapshotWriteJob<ContentDomainObject.State> job,
            CancellationToken ct = default)
        {
            var entity = await MongoContentEntity.CreatePublishedAsync(job, appProvider);

            await collectionPublished.UpsertVersionedAsync(entity.DocumentId, job.OldVersion, entity, job.Action, ct);
        }

        private static bool ShouldWritePublished(ContentDomainObject.State value)
        {
            // Only published content is written to the published collection.
            return value.Status == Status.Published && !value.IsDeleted;
        }

        private static bool IsValid(SnapshotWriteJob<ContentDomainObject.State> job)
        {
            // Some data is corrupt and might throw an exception during migration if we do not skip them.
            return job.Value.AppId != null || job.Value.CurrentVersion != null;
        }
    }
}
