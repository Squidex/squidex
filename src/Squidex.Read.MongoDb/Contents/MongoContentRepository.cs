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
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Contents;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.MongoDb.Contents.Visitors;
using Squidex.Read.Schemas.Services;

namespace Squidex.Read.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IEventConsumer
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly ISchemaProvider schemaProvider;

        protected IndexKeysDefinitionBuilder<MongoContentEntity> IndexKeys
        {
            get
            {
                return Builders<MongoContentEntity>.IndexKeys;
            }
        }

        public MongoContentRepository(IMongoDatabase database, ISchemaProvider schemaProvider)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNull(schemaProvider, nameof(schemaProvider));

            this.database = database;

            this.schemaProvider = schemaProvider;
        }

        public async Task<List<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages)
        {
            List<IContentEntity> result = null;

            await ForSchemaAsync(schemaId, async (collection, schema) =>
            {
                IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
                try
                {
                    var parser = schema.ParseQuery(languages, odataQuery);

                    cursor = collection.Find(parser, schema, nonPublished).Take(parser).Skip(parser).Sort(parser, schema);
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
                    entity.ParseData(schema);
                }

                result = entities.OfType<IContentEntity>().ToList();
            });

            return result;
        }

        public async Task<long> CountAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages)
        {
            var result = 0L;

            await ForSchemaAsync(schemaId, async (collection, schema) =>
            {
                IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
                try
                {
                    var parser = schema.ParseQuery(languages, odataQuery);

                    cursor = collection.Find(parser, schema, nonPublished);
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

            await ForSchemaAsync(schemaId, async (collection, schema) =>
            {
                result = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

                result?.ParseData(schema);
            });

            return result;
        }

        private async Task ForSchemaAsync(Guid schemaId, Func<IMongoCollection<MongoContentEntity>, Schema, Task> action)
        {
            var collection = GetCollection(schemaId);

            var schemaEntity = await schemaProvider.FindSchemaByIdAsync(schemaId);

            if (schemaEntity == null)
            {
                return;
            }

            await action(collection, schemaEntity.Schema);
        }
    }
}
