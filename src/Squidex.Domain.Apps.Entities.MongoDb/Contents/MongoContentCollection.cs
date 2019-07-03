// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    internal class MongoContentCollection : MongoRepositoryBase<MongoContentEntity>
    {
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;

        public MongoContentCollection(IMongoDatabase database, IJsonSerializer serializer, IAppProvider appProvider)
            : base(database)
        {
            this.appProvider = appProvider;

            this.serializer = serializer;
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.Id)),
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.Id)),
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.ScheduledAt)
                    .Ascending(x => x.IsDeleted)),
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.ReferencedIds))
            }, ct);
        }

        protected override string CollectionName()
        {
            return "State_Contents";
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(ISchemaEntity schema, Query query, List<Guid> ids, Status[] status, bool inDraft, bool includeDraft = true)
        {
            try
            {
                query = query.AdjustToModel(schema.SchemaDef, inDraft);

                var filter = query.ToFilter(schema.Id, ids, status);

                var contentCount = Collection.Find(filter).CountDocumentsAsync();
                var contentItems =
                    Collection.Find(filter)
                        .WithoutDraft(includeDraft)
                        .ContentTake(query)
                        .ContentSkip(query)
                        .ContentSort(query)
                        .ToListAsync();

                await Task.WhenAll(contentItems, contentCount);

                foreach (var entity in contentItems.Result)
                {
                    entity.ParseData(schema.SchemaDef, serializer);
                }

                return ResultList.Create<IContentEntity>(contentCount.Result, contentItems.Result);
            }
            catch (MongoQueryException ex)
            {
                if (ex.Message.Contains("17406"))
                {
                    throw new DomainException("Result set is too large to be retrieved. Use $top parameter to reduce the number of items.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, HashSet<Guid> ids, Status[] status, bool includeDraft)
        {
            var find = Collection.Find(FilterFactory.IdsByApp(app.Id, ids, status));

            var contentItems = await find.WithoutDraft(includeDraft).ToListAsync();

            var schemaIds = contentItems.Select(x => x.IndexedSchemaId).ToList();
            var schemas = await Task.WhenAll(schemaIds.Select(x => appProvider.GetSchemaAsync(app.Id, x)));

            var result = new List<(IContentEntity Content, ISchemaEntity Schema)>();

            foreach (var entity in contentItems)
            {
                var schema = schemas.FirstOrDefault(x => x.Id == entity.IndexedSchemaId);

                if (schema != null)
                {
                    entity.ParseData(schema.SchemaDef, serializer);

                    result.Add((entity, schema));
                }
            }

            return result;
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(ISchemaEntity schema, HashSet<Guid> ids, Status[] status, bool includeDraft)
        {
            var find = Collection.Find(FilterFactory.IdsBySchema(schema.Id, ids, status));

            var contentItems = await find.WithoutDraft(includeDraft).ToListAsync();

            foreach (var entity in contentItems)
            {
                entity.ParseData(schema.SchemaDef, serializer);
            }

            return ResultList.Create<IContentEntity>(contentItems.Count, contentItems);
        }

        public async Task<IContentEntity> FindContentAsync(ISchemaEntity schema, Guid id, Status[] status, bool includeDraft)
        {
            var find = Collection.Find(FilterFactory.Build(schema.Id, id, status));

            var contentEntity = await find.WithoutDraft(includeDraft).FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef, serializer);

            return contentEntity;
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return Collection.Find(x => x.ScheduledAt < now && x.IsDeleted != true)
                .Not(x => x.DataByIds)
                .Not(x => x.DataDraftByIds)
                .ForEachAsync(c =>
                {
                    callback(c);
                });
        }

        public async Task<IReadOnlyList<Guid>> QueryIdsAsync(ISchemaEntity schema, FilterNode filterNode)
        {
            var filter = filterNode.AdjustToModel(schema.SchemaDef, true).ToFilter(schema.Id);

            var contentEntities =
                await Collection.Find(filter).Only(x => x.Id)
                    .ToListAsync();

            return contentEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<IReadOnlyList<Guid>> QueryIdsAsync(Guid appId)
        {
            var contentEntities =
                await Collection.Find(x => x.IndexedAppId == appId).Only(x => x.Id)
                    .ToListAsync();

            return contentEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<(ContentState Value, long Version)> ReadAsync(Guid key, Func<Guid, Guid, Task<ISchemaEntity>> getSchema)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (contentEntity != null)
            {
                var schema = await getSchema(contentEntity.IndexedAppId, contentEntity.IndexedSchemaId);

                contentEntity.ParseData(schema.SchemaDef, serializer);

                return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
            }

            return (null, EtagVersion.NotFound);
        }

        public Task ReadAllAsync(Func<ContentState, long, Task> callback, Func<Guid, Guid, Task<ISchemaEntity>> getSchema, CancellationToken ct = default)
        {
            return Collection.Find(new BsonDocument()).ForEachPipelineAsync(async contentEntity =>
            {
                var schema = await getSchema(contentEntity.IndexedAppId, contentEntity.IndexedSchemaId);

                contentEntity.ParseData(schema.SchemaDef, serializer);

                await callback(SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
            }, ct);
        }

        public Task CleanupAsync(Guid id)
        {
            return Collection.UpdateManyAsync(
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, id),
                    Filter.AnyNe(x => x.ReferencedIdsDeleted, id)),
                Update.AddToSet(x => x.ReferencedIdsDeleted, id));
        }

        public Task RemoveAsync(Guid id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task UpsertAsync(MongoContentEntity content, long oldVersion)
        {
            try
            {
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
