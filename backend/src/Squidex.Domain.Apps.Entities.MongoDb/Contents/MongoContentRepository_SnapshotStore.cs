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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentState, Guid>
    {
        Task ISnapshotStore<ContentState, Guid>.ReadAllAsync(Func<ContentState, long, Task> callback, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        async Task ISnapshotStore<ContentState, Guid>.ClearAsync()
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.ClearAsync();
                await collectionPublished.ClearAsync();
            }
        }

        async Task ISnapshotStore<ContentState, Guid>.RemoveAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.RemoveAsync(key);
                await collectionPublished.RemoveAsync(key);
            }
        }

        async Task<(ContentState Value, long Version)> ISnapshotStore<ContentState, Guid>.ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                var contentEntity = await collectionAll.FindAsync(key);

                if (contentEntity != null)
                {
                    var schema = await GetSchemaAsync(contentEntity.IndexedAppId, contentEntity.IndexedSchemaId);

                    contentEntity.ParseData(schema.SchemaDef, serializer);

                    return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
                }

                return (null!, EtagVersion.NotFound);
            }
        }

        async Task ISnapshotStore<ContentState, Guid>.WriteAsync(Guid key, ContentState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (value.SchemaId.Id == Guid.Empty)
                {
                    return;
                }

                var schema = await GetSchemaAsync(value.AppId.Id, value.SchemaId.Id);

                await UpsertDraftContentAsync(value, oldVersion, newVersion, schema);

                if (value.Status == Status.Published)
                {
                    await UpsertPublishedContentAsync(value, oldVersion, newVersion, schema);
                }
                else
                {
                    await DeletePublishedContentAsync(key);
                }
            }
        }

        private Task DeletePublishedContentAsync(Guid key)
        {
            return collectionPublished.RemoveAsync(key);
        }

        private async Task UpsertDraftContentAsync(ContentState value, long oldVersion, long newVersion, ISchemaEntity schema)
        {
            var content = SimpleMapper.Map(value, new MongoContentEntity
            {
                IndexedAppId = value.AppId.Id,
                IndexedSchemaId = value.SchemaId.Id,
                Version = newVersion
            });

            content.ScheduledAt = value.ScheduleJob?.DueTime;
            content.ScheduleJob = value.ScheduleJob;
            content.NewStatus = value.NewStatus;

            content.LoadData(value.EditingData, schema.SchemaDef, serializer);

            await collectionAll.UpsertVersionedAsync(content.Id, oldVersion, content);
        }

        private async Task UpsertPublishedContentAsync(ContentState value, long oldVersion, long newVersion, ISchemaEntity schema)
        {
            var content = SimpleMapper.Map(value, new MongoContentEntity
            {
                IndexedAppId = value.AppId.Id,
                IndexedSchemaId = value.SchemaId.Id,
                Version = newVersion
            });

            content.ScheduledAt = null;
            content.ScheduleJob = null;
            content.NewStatus = null;

            content.LoadData(value.Data, schema.SchemaDef, serializer);

            await collectionPublished.UpsertVersionedAsync(content.Id, oldVersion, content);
        }

        private async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid schemaId)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId, true);

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaId.ToString(), typeof(ISchemaEntity));
            }

            return schema;
        }
    }
}
