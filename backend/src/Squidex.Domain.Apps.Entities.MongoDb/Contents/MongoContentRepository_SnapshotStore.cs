// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentDomainObject.State>, IDeleter
    {
        IAsyncEnumerable<(ContentDomainObject.State State, long Version)> ISnapshotStore<ContentDomainObject.State>.ReadAllAsync(
            CancellationToken ct)
        {
            return AsyncEnumerable.Empty<(ContentDomainObject.State State, long Version)>();
        }

        async Task<(ContentDomainObject.State Value, bool Valid, long Version)> ISnapshotStore<ContentDomainObject.State>.ReadAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/ReadAsync"))
            {
                var version = await collectionAll.FindVersionAsync(key, ct);

                return (null!, false, version);
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

        async Task ISnapshotStore<ContentDomainObject.State>.WriteAsync(DomainId key, ContentDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteAsync"))
            {
                if (value.SchemaId.Id == DomainId.Empty)
                {
                    return;
                }

                await Task.WhenAll(
                    UpsertDraftContentAsync(value, oldVersion, newVersion, ct),
                    UpsertOrDeletePublishedAsync(value, oldVersion, newVersion, ct));
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.WriteManyAsync(IEnumerable<(DomainId Key, ContentDomainObject.State Value, long Version)> snapshots,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteManyAsync"))
            {
                var entitiesPublished = new List<MongoContentEntity>();
                var entitiesAll = new List<MongoContentEntity>();

                foreach (var (_, value, version) in snapshots)
                {
                    if (ShouldWritePublished(value))
                    {
                        entitiesPublished.Add(await CreatePublishedContentAsync(value, version));
                    }

                    entitiesAll.Add(await CreateDraftContentAsync(value, version));
                }

                await Task.WhenAll(
                    collectionPublished.InsertManyAsync(entitiesPublished, ct),
                    collectionAll.InsertManyAsync(entitiesAll, ct));
            }
        }

        private async Task UpsertOrDeletePublishedAsync(ContentDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct = default)
        {
            if (ShouldWritePublished(value))
            {
                await UpsertPublishedContentAsync(value, oldVersion, newVersion, ct);
            }
            else
            {
                await DeletePublishedContentAsync(value.AppId.Id, value.Id, ct);
            }
        }

        private Task DeletePublishedContentAsync(DomainId appId, DomainId id,
            CancellationToken ct = default)
        {
            var documentId = DomainId.Combine(appId, id);

            return collectionPublished.RemoveAsync(documentId, ct);
        }

        private async Task UpsertDraftContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct = default)
        {
            var entity = await CreateDraftContentAsync(value, newVersion);

            await collectionAll.UpsertVersionedAsync(entity.DocumentId, oldVersion, entity, ct);
        }

        private async Task UpsertPublishedContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct = default)
        {
            var entity = await CreatePublishedContentAsync(value, newVersion);

            await collectionPublished.UpsertVersionedAsync(entity.DocumentId, oldVersion, entity, ct);
        }

        private async Task<MongoContentEntity> CreatePublishedContentAsync(ContentDomainObject.State value, long newVersion)
        {
            var entity = await CreateContentAsync(value, value.CurrentVersion.Data, newVersion);

            entity.ScheduledAt = null;
            entity.ScheduleJob = null;
            entity.NewStatus = null;

            return entity;
        }

        private async Task<MongoContentEntity> CreateDraftContentAsync(ContentDomainObject.State value, long newVersion)
        {
            var entity = await CreateContentAsync(value, value.Data, newVersion);

            entity.ScheduledAt = value.ScheduleJob?.DueTime;
            entity.ScheduleJob = value.ScheduleJob;
            entity.NewStatus = value.NewStatus;

            return entity;
        }

        private async Task<MongoContentEntity> CreateContentAsync(ContentDomainObject.State value, ContentData data, long newVersion)
        {
            var entity = SimpleMapper.Map(value, new MongoContentEntity());

            entity.Data = data;
            entity.DocumentId = value.UniqueId;
            entity.IndexedAppId = value.AppId.Id;
            entity.IndexedSchemaId = value.SchemaId.Id;
            entity.Version = newVersion;

            var schema = await appProvider.GetSchemaAsync(value.AppId.Id, value.SchemaId.Id, true);

            if (schema != null)
            {
                var components = await appProvider.GetComponentsAsync(schema);

                entity.ReferencedIds = entity.Data.GetReferencedIds(schema.SchemaDef, components);
            }
            else
            {
                entity.ReferencedIds = new HashSet<DomainId>();
            }

            return entity;
        }

        private static bool ShouldWritePublished(ContentDomainObject.State value)
        {
            return value.Status == Status.Published && !value.IsDeleted;
        }
    }
}
