// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    internal sealed class MongoContentDraftCollection : MongoContentCollection
    {
        public MongoContentDraftCollection(IMongoDatabase database)
            : base(database, "State_Content_Draft")
        {
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.Id)
                    .Ascending(x => x.IsDeleted));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Text(x => x.DataText)
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status));

            await base.SetupCollectionAsync(collection);
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> ids)
        {
            var contentEntities =
                await Collection.Find(x => x.IndexedSchemaId == schemaId && ids.Contains(x.Id) && x.IsDeleted != true).Only(x => x.Id)
                    .ToListAsync();

            return ids.Except(contentEntities.Select(x => Guid.Parse(x["_id"].AsString))).ToList();
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return Collection.Find(x => x.ScheduledAt < now && x.IsDeleted != true)
                .Not(x => x.DataByIds)
                .Not(x => x.DataDraftByIds)
                .Not(x => x.DataText)
                .ForEachAsync(c =>
                {
                    callback(c);
                });
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var contentEntity =
                await Collection.Find(x => x.IndexedSchemaId == schema.Id && x.Id == id && x.IsDeleted != true).Not(x => x.DataText)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public async Task<(ContentState Value, long Version)> ReadAsync(Guid key, Func<Guid, Guid, Task<ISchemaEntity>> getSchema)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == key).Not(x => x.DataText)
                    .FirstOrDefaultAsync();

            if (contentEntity != null)
            {
                var schema = await getSchema(contentEntity.IndexedAppId, contentEntity.IndexedSchemaId);

                contentEntity?.ParseData(schema.SchemaDef);

                return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
            }

            return (null, EtagVersion.NotFound);
        }

        public async Task UpsertAsync(MongoContentEntity content, long oldVersion)
        {
            try
            {
                content.DataText = content.DataDraftByIds.ToFullText();

                await Collection.ReplaceOneAsync(x => x.Id == content.Id && x.Version == oldVersion, content, Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == content.Id).Only(x => x.Id, x => x.Version)
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
        }
    }
}
