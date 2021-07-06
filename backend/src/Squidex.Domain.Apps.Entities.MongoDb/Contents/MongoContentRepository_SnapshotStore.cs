// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentDomainObject.State>
    {
        Task ISnapshotStore<ContentDomainObject.State>.ReadAllAsync(Func<ContentDomainObject.State, long, Task> callback,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        async Task<(ContentDomainObject.State Value, bool Valid, long Version)> ISnapshotStore<ContentDomainObject.State>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                var version = await collectionAll.FindVersionAsync(key);

                return (null!, false, version);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.ClearAsync()
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.ClearAsync();
                await collectionPublished.ClearAsync();
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.RemoveAsync(key);
                await collectionPublished.RemoveAsync(key);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.WriteAsync(DomainId key, ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (value.SchemaId.Id == DomainId.Empty)
                {
                    return;
                }

                await Task.WhenAll(
                    UpsertDraftContentAsync(value, oldVersion, newVersion),
                    UpsertOrDeletePublishedAsync(value, oldVersion, newVersion));
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State>.WriteManyAsync(IEnumerable<(DomainId Key, ContentDomainObject.State Value, long Version)> snapshots)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
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
                    collectionPublished.InsertManyAsync(entitiesPublished),
                    collectionAll.InsertManyAsync(entitiesAll));
            }
        }

        private async Task UpsertOrDeletePublishedAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            if (ShouldWritePublished(value))
            {
                await UpsertPublishedContentAsync(value, oldVersion, newVersion);
            }
            else
            {
                await DeletePublishedContentAsync(value.AppId.Id, value.Id);
            }
        }

        private Task DeletePublishedContentAsync(DomainId appId, DomainId id)
        {
            var documentId = DomainId.Combine(appId, id);

            return collectionPublished.RemoveAsync(documentId);
        }

        private async Task UpsertDraftContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            var entity = await CreateDraftContentAsync(value, newVersion);

            await collectionAll.UpsertVersionedAsync(entity.DocumentId, oldVersion, entity);
        }

        private async Task UpsertPublishedContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            var entity = await CreatePublishedContentAsync(value, newVersion);

            await collectionPublished.UpsertVersionedAsync(entity.DocumentId, oldVersion, entity);
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
