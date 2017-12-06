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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public class MongoContentRepository : IContentRepository
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly IAppProvider appProvider;

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

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.database = database;
            this.appProvider = appProvider;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            var collection = GetCollection(app.Id);

            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor =
                    collection
                        .Find(odataQuery, schema.Id, schema.SchemaDef, status)
                        .Take(odataQuery)
                        .Skip(odataQuery)
                        .Sort(odataQuery, schema.SchemaDef);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }

            var contentEntities = await cursor.ToListAsync();

            foreach (var entity in contentEntities)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return contentEntities;
        }

        public Task<long> CountAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            var collection = GetCollection(app.Id);

            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor = collection.Find(odataQuery, schema.Id, schema.SchemaDef, status);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }

            return cursor.CountAsync();
        }

        public async Task<long> CountAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var collection = GetCollection(app.Id);

            var contentsCount =
                await collection.Find(x => ids.Contains(x.Id))
                    .CountAsync();

            return contentsCount;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var collection = GetCollection(app.Id);

            var contentEntities =
                await collection.Find(x => ids.Contains(x.Id))
                    .ToListAsync();

            foreach (var entity in contentEntities)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return contentEntities.OfType<IContentEntity>().ToList();
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds)
        {
            var collection = GetCollection(appId);

            var contentEntities =
                await collection.Find(x => contentIds.Contains(x.Id) && x.AppId == appId).Project<BsonDocument>(Projection.Include(x => x.Id))
                    .ToListAsync();

            return contentIds.Except(contentEntities.Select(x => Guid.Parse(x["_id"].AsString))).ToList();
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var collection = GetCollection(app.Id);

            var contentEntity =
                await collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        private async Task ForSchemaAsync(NamedId<Guid> appId, Guid schemaId, Func<IMongoCollection<MongoContentEntity>, ISchemaEntity, Task> action)
        {
            var collection = GetCollection(appId.Id);

            var schema = await appProvider.GetSchemaAsync(appId.Name, schemaId, true);

            if (schema == null)
            {
                return;
            }

            await action(collection, schema);
        }

        private Task ForAppIdAsync(Guid appId, Func<IMongoCollection<MongoContentEntity>, Task> action)
        {
            var collection = GetCollection(appId);

            return action(collection);
        }

        private IMongoCollection<MongoContentEntity> GetCollection(Guid appId)
        {
            var name = $"{Prefix}{appId}";

            return database.GetCollection<MongoContentEntity>(name);
        }
    }
}
