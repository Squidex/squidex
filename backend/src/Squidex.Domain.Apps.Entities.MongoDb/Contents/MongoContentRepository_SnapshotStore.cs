// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentState, DomainId>
    {
        Task ISnapshotStore<ContentState, DomainId>.ReadAllAsync(Func<ContentState, long, Task> callback, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        async Task ISnapshotStore<ContentState, DomainId>.ClearAsync()
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.ClearAsync();
                await collectionPublished.ClearAsync();
            }
        }

        async Task ISnapshotStore<ContentState, DomainId>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.RemoveAsync(key);
                await collectionPublished.RemoveAsync(key);
            }
        }

        async Task<(ContentState Value, long Version)> ISnapshotStore<ContentState, DomainId>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                var contentEntity = await collectionAll.FindAsync(key);

                if (contentEntity != null)
                {
                    var schema = await GetSchemaAsync(contentEntity.IndexedAppId, contentEntity.IndexedSchemaId);

                    if (schema == null)
                    {
                        return (null!, EtagVersion.NotFound);
                    }

                    contentEntity.ParseData(schema.SchemaDef, converter);

                    return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
                }

                return (null!, EtagVersion.NotFound);
            }
        }

        async Task ISnapshotStore<ContentState, DomainId>.WriteAsync(DomainId key, ContentState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (value.SchemaId.Id == DomainId.Empty)
                {
                    return;
                }

                var schema = await GetSchemaAsync(value.AppId.Id, value.SchemaId.Id);

                if (schema == null)
                {
                    return;
                }

                var saveDraft = UpsertDraftContentAsync(value, oldVersion, newVersion, schema);
                var savePublic = UpsertOrDeletePublishedAsync(value, oldVersion, newVersion, schema);

                await Task.WhenAll(saveDraft, savePublic);
            }
        }

        private async Task UpsertOrDeletePublishedAsync(ContentState value, long oldVersion, long newVersion, ISchemaEntity schema)
        {
            if (value.Status == Status.Published && !value.IsDeleted)
            {
                await UpsertPublishedContentAsync(value, oldVersion, newVersion, schema);
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

        private async Task UpsertDraftContentAsync(ContentState value, long oldVersion, long newVersion, ISchemaEntity schema)
        {
            var content = SimpleMapper.Map(value, new MongoContentEntity
            {
                IndexedAppId = value.AppId.Id,
                IndexedSchemaId = value.SchemaId.Id,
                Version = newVersion
            });

            content.DocumentId = value.UniqueId;
            content.ScheduledAt = value.ScheduleJob?.DueTime;
            content.ScheduleJob = value.ScheduleJob;
            content.NewStatus = value.NewStatus;

            content.LoadData(value.Data, schema.SchemaDef, converter);

            await collectionAll.UpsertVersionedAsync(content.DocumentId, oldVersion, content);
        }

        private async Task UpsertPublishedContentAsync(ContentState value, long oldVersion, long newVersion, ISchemaEntity schema)
        {
            var content = SimpleMapper.Map(value, new MongoContentEntity
            {
                IndexedAppId = value.AppId.Id,
                IndexedSchemaId = value.SchemaId.Id,
                Version = newVersion
            });

            content.DocumentId = value.UniqueId;
            content.ScheduledAt = null;
            content.ScheduleJob = null;
            content.NewStatus = null;

            content.LoadData(value.CurrentVersion.Data, schema.SchemaDef, converter);

            await collectionPublished.UpsertVersionedAsync(content.DocumentId, oldVersion, content);
        }

        private async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId schemaId)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId, true);

            return schema;
        }
    }
}
