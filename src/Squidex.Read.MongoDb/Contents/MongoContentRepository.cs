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
using Microsoft.OData.Core;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Contents;
using Squidex.Read.Contents.Builders;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.MongoDb.Contents.Visitors;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;

namespace Squidex.Read.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IEventConsumer
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly ISchemaProvider schemas;
        private readonly EdmModelBuilder modelBuilder;

        protected static IndexKeysDefinitionBuilder<MongoContentEntity> IndexKeys
        {
            get
            {
                return Builders<MongoContentEntity>.IndexKeys;
            }
        }

        public MongoContentRepository(IMongoDatabase database, ISchemaProvider schemas, EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(schemas, nameof(schemas));

            this.schemas = schemas;
            this.database = database;
            this.modelBuilder = modelBuilder;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages)
        {
            List<IContentEntity> result = null;

            await ForSchemaAsync(schemaId, async (collection, schemaEntity) =>
            {
                IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
                try
                {
                    var model = modelBuilder.BuildEdmModel(schemaEntity, languages);

                    var parser = model.ParseQuery(odataQuery);

                    cursor = collection.Find(parser, schemaEntity.Schema, nonPublished).Take(parser).Skip(parser).Sort(parser, schemaEntity.Schema);
                }
                catch (NotSupportedException)
                {
                    throw new ValidationException("This odata operation is not supported");
                }
                catch (NotImplementedException)
                {
                    throw new ValidationException("This odata operation is not supported");
                }
                catch (ODataException ex)
                {
                    throw new ValidationException("Failed to parse query: " + ex.Message, ex);
                }

                var entities = await cursor.ToListAsync();

                foreach (var entity in entities)
                {
                    entity.ParseData(schemaEntity.Schema);
                }

                result = entities.OfType<IContentEntity>().ToList();
            });

            return result;
        }

        public async Task<long> CountAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages)
        {
            var result = 0L;

            await ForSchemaAsync(schemaId, async (collection, schemaEntity) =>
            {
                IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
                try
                {
                    var model = modelBuilder.BuildEdmModel(schemaEntity, languages);

                    var parser = model.ParseQuery(odataQuery);

                    cursor = collection.Find(parser, schemaEntity.Schema, nonPublished);
                }
                catch (NotSupportedException)
                {
                    throw new ValidationException("This odata operation is not supported");
                }
                catch (NotImplementedException)
                {
                    throw new ValidationException("This odata operation is not supported");
                }
                catch (ODataException ex)
                {
                    throw new ValidationException("Failed to parse query: " + ex.Message, ex);
                }

                result = await cursor.CountAsync();
            });

            return result;
        }

        public async Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id)
        {
            MongoContentEntity result = null;

            await ForSchemaAsync(schemaId, async (collection, schemaEntity) =>
            {
                result = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

                result?.ParseData(schemaEntity.Schema);
            });

            return result;
        }

        private async Task ForSchemaAsync(Guid schemaId, Func<IMongoCollection<MongoContentEntity>, ISchemaEntityWithSchema, Task> action)
        {
            var collection = GetCollection(schemaId);

            var schemaEntity = await schemas.FindSchemaByIdAsync(schemaId);

            if (schemaEntity == null)
            {
                return;
            }

            await action(collection, schemaEntity);
        }
    }
}
