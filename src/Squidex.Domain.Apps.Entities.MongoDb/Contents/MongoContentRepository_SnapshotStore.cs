// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentState, Guid>
    {
        public Task<(ContentState Value, long Version)> ReadAsync(Guid key)
        {
            return contentsDraft.ReadAsync(key, GetSchemaAsync);
        }

        public async Task WriteAsync(Guid key, ContentState value, long oldVersion, long newVersion)
        {
            if (value.SchemaId.Id == Guid.Empty)
            {
                return;
            }

            var schema = await GetSchemaAsync(value.AppId.Id, value.SchemaId.Id);

            var idData = value.Data.ToIdModel(schema.SchemaDef, true);

            var content = SimpleMapper.Map(value, new MongoContentEntity
            {
                DataByIds = idData,
                DataDraftByIds = value.DataDraft?.ToIdModel(schema.SchemaDef, true),
                IsDeleted = value.IsDeleted,
                IndexedAppId = value.AppId.Id,
                IndexedSchemaId = value.SchemaId.Id,
                ReferencedIds = idData.ToReferencedIds(schema.SchemaDef),
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

        private async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid schemaId)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId, true);

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaId.ToString(), typeof(ISchemaEntity));
            }

            return schema;
        }

        Task ISnapshotStore<ContentState, Guid>.ReadAllAsync(Func<ContentState, long, Task> callback)
        {
            throw new NotSupportedException();
        }
    }
}
