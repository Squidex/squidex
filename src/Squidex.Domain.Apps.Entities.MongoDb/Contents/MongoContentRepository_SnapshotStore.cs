// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentState, Guid>
    {
        public async Task<(ContentState Value, long Version)> ReadAsync(Guid key)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == key).SortByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

            if (contentEntity != null)
            {
                var schema = await GetSchemaAsync(contentEntity.AppIdId, contentEntity.SchemaIdId);

                contentEntity?.ParseData(schema.SchemaDef);

                return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
            }

            return (null, EtagVersion.NotFound);
        }

        public async Task WriteAsync(Guid key, ContentState value, long oldVersion, long newVersion)
        {
            if (value.SchemaId.Id == Guid.Empty)
            {
                return;
            }

            var schema = await GetSchemaAsync(value.AppId.Id, value.SchemaId.Id);

            var idData = value.Data?.ToIdModel(schema.SchemaDef, true);

            var id = key.ToString();

            var document = SimpleMapper.Map(value, new MongoContentEntity
            {
                AppIdId = value.AppId.Id,
                SchemaIdId = value.SchemaId.Id,
                IsDeleted = value.IsDeleted,
                DocumentId = key.ToString(),
                DataText = idData?.ToFullText(),
                DataByIds = idData,
                ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef),
            });

            document.Version = newVersion;

            try
            {
                await Collection.ReplaceOneAsync(x => x.DocumentId == id && x.Version == oldVersion, document, Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.DocumentId == id).Only(x => x.DocumentId, x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion["vs"].AsInt64, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }

            document.DocumentId = $"{key}_{newVersion}";

            await ArchiveCollection.ReplaceOneAsync(x => x.DocumentId == document.DocumentId, document, Upsert);
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
