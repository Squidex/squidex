﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
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
        async Task ISnapshotStore<ContentState, Guid>.RemoveAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await contents.RemoveAsync(key);
            }
        }

        async Task ISnapshotStore<ContentState, Guid>.ReadAllAsync(Func<ContentState, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await contents.ReadAllAsync(callback, GetSchemaAsync, ct);
            }
        }

        async Task<(ContentState Value, long Version)> ISnapshotStore<ContentState, Guid>.ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contents.ReadAsync(key, GetSchemaAsync);
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

                var idData = value.Data!.ToMongoModel(schema.SchemaDef, serializer);
                var idDraftData = idData;

                if (!ReferenceEquals(value.Data, value.DataDraft))
                {
                    idDraftData = value.DataDraft.ToMongoModel(schema.SchemaDef, serializer);
                }

                var content = SimpleMapper.Map(value, new MongoContentEntity
                {
                    DataByIds = idData,
                    DataDraftByIds = idDraftData,
                    IsDeleted = value.IsDeleted,
                    IndexedAppId = value.AppId.Id,
                    IndexedSchemaId = value.SchemaId.Id,
                    ReferencedIds = idData.ToReferencedIds(schema.SchemaDef),
                    ScheduledAt = value.ScheduleJob?.DueTime,
                    Version = newVersion
                });

                await contents.UpsertAsync(content, oldVersion);
            }
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
