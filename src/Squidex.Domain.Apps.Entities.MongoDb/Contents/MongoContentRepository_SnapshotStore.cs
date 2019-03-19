// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentState, Guid>
    {
        public async Task<(ContentState Value, long Version)> ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contentsDraft.ReadAsync(key, GetSchemaAsync);
            }
        }

        public async Task WriteAsync(Guid key, ContentState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (value.SchemaId.Id == Guid.Empty)
                {
                    return;
                }

                var schema = await GetSchemaAsync(value.AppId.Id, value.SchemaId.Id);
                //calculate only on inserts, updates would be already stored
                if (newVersion == 0)
                {
                    if (value.OrderNo == null || value.OrderNo == 0)
                    {
                        value.OrderNo = await CalculateNewOrderNoAsync(schema);
                    }
                }

                var idData = value.Data.ToMongoModel(schema.SchemaDef, serializer);
                var idDraftData = idData;

                if (!ReferenceEquals(value.Data, value.DataDraft))
                {
                    idDraftData = value.DataDraft?.ToMongoModel(schema.SchemaDef, serializer);
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

                await contentsDraft.UpsertAsync(content, oldVersion);

                if (value.Status == Status.Published && !value.IsDeleted)
                {
                    await contentsPublished.UpsertAsync(content);
                }
                else
                {
                    await contentsPublished.RemoveAsync(content.Id);
                }
            }
        }

        /// <summary>
        /// new orderNo fields must be calculated in ascending order, such as 1,2,3,4 as contents are created.
        /// By this way, we can switch order no values from ui by drag-drop actions.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private async Task<long?> CalculateNewOrderNoAsync(ISchemaEntity schema)
        {
            var query = new Query
            {
                Take = 1,
                Sort = new List<SortNode>
                {
                    new SortNode(new List<string> { nameof(IContentEntity.OrderNo) }, SortOrder.Descending)
                }
            };
            var maxDraftContent = await contentsDraft.QueryAsync(null, schema, query);
            var maxDraftOrderNo = maxDraftContent.FirstOrDefault()?.OrderNo ?? 0;

            var maxPublishedContent = await contentsPublished.QueryAsync(null, schema, query);
            var maxPublishedOrderNo = maxPublishedContent.FirstOrDefault()?.OrderNo ?? 0;

            return Math.Max(maxDraftOrderNo, maxPublishedOrderNo) + 1;
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

        Task ISnapshotStore<ContentState, Guid>.RemoveAsync(Guid key)
        {
            throw new NotSupportedException();
        }

        Task ISnapshotStore<ContentState, Guid>.ReadAllAsync(Func<ContentState, long, Task> callback)
        {
            throw new NotSupportedException();
        }
    }
}
