// ==========================================================================
//  MongoContentRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IEventConsumer
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly ISchemaProvider schemas;

        protected static FilterDefinitionBuilder<MongoContentEntity> Filter
        {
            get
            {
                return Builders<MongoContentEntity>.Filter;
            }
        }

        protected static UpdateDefinitionBuilder<MongoContentEntity> Update
        {
            get
            {
                return Builders<MongoContentEntity>.Update;
            }
        }

        protected static ProjectionDefinitionBuilder<MongoContentEntity> Projection
        {
            get
            {
                return Builders<MongoContentEntity>.Projection;
            }
        }

        protected static IndexKeysDefinitionBuilder<MongoContentEntity> Index
        {
            get
            {
                return Builders<MongoContentEntity>.IndexKeys;
            }
        }

        public MongoContentRepository(IMongoDatabase database, ISchemaProvider schemas)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNull(schemas, nameof(schemas));

            this.schemas = schemas;
            this.database = database;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity appEntity, ISchemaEntity schemaEntity, bool nonPublished, HashSet<Guid> ids, ODataUriParser odataQuery)
        {
            var collection = GetCollection(appEntity.Id);

            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor =
                    collection
                        .Find(odataQuery, ids, schemaEntity.Id, schemaEntity.Schema, nonPublished)
                        .Take(odataQuery)
                        .Skip(odataQuery)
                        .Sort(odataQuery, schemaEntity.Schema);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported");
            }

            var entities = await cursor.ToListAsync();

            foreach (var entity in entities)
            {
                entity.ParseData(schemaEntity.Schema);
            }

            return entities;
        }

        public Task<long> CountAsync(IAppEntity appEntity, ISchemaEntity schemaEntity, bool nonPublished, HashSet<Guid> ids, ODataUriParser odataQuery)
        {
            var collection = GetCollection(appEntity.Id);

            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor = collection.Find(odataQuery, ids, schemaEntity.Id, schemaEntity.Schema, nonPublished);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported");
            }

            return cursor.CountAsync();
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds)
        {
            var collection = GetCollection(appId);

            var contentEntities =
                await collection.Find(x => contentIds.Contains(x.Id) && x.AppId == appId).Project<BsonDocument>(Projection.Include(x => x.Id))
                    .ToListAsync();

            return contentIds.Except(contentEntities.Select(x => Guid.Parse(x["_id"].AsString))).ToList();
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity appEntity, ISchemaEntity schemaEntity, Guid id)
        {
            var collection = GetCollection(appEntity.Id);

            var contentEntity =
                await collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schemaEntity.Schema);

            return contentEntity;
        }

        private async Task ForSchemaAsync(Guid appId, Guid schemaId, Func<IMongoCollection<MongoContentEntity>, ISchemaEntity, Task> action)
        {
            var collection = GetCollection(appId);

            var schemaEntity = await schemas.FindSchemaByIdAsync(schemaId);

            if (schemaEntity == null)
            {
                return;
            }

            await action(collection, schemaEntity);
        }
    }
}
