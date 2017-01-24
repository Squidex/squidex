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
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Core.Schemas;
using Squidex.Events;
using Squidex.Events.Contents;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Replay;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Contents;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.Schemas.Services;
using Squidex.Read.MongoDb.Contents.Visitors;
using Squidex.Read.MongoDb.Utils;

namespace Squidex.Read.MongoDb.Contents
{
    public class MongoContentRepository : IContentRepository, ICatchEventConsumer, IReplayableStore
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly ISchemaProvider schemaProvider;
        
        protected UpdateDefinitionBuilder<MongoContentEntity> Update
        {
            get
            {
                return Builders<MongoContentEntity>.Update;
            }
        }

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

        public async Task ClearAsync()
        {
            using (var collections = await database.ListCollectionsAsync())
            {
                while (await collections.MoveNextAsync())
                {
                    foreach (var collection in collections.Current)
                    {
                        var name = collection["name"].ToString();

                        if (name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            await database.DropCollectionAsync(name);
                        }
                    }
                }
            }
        }

        public async Task<List<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, string odataQuery, HashSet<Language> languages)
        {
            List<IContentEntity> result = null;

            await ForSchemaAsync(schemaId, async (collection, schema) =>
            {
                var parser = schema.ParseQuery(languages, odataQuery);
                var cursor = collection.Find(parser, schema, nonPublished).Take(parser).Skip(parser).Sort(parser, schema);

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
                var parser = schema.ParseQuery(languages, odataQuery);
                var cursor = collection.Find(parser, schema, nonPublished);

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

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers.SchemaId(), (collection, schema) =>
            {
                return collection.CreateAsync(headers, x =>
                {
                    SimpleMapper.Map(@event, x);

                    x.SetData(schema, @event.Data);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers.SchemaId(), (collection, schema) =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.SetData(schema, @event.Data);
                });
            });
        }

        protected Task On(ContentPublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = true;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = false;
                });
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsDeleted = true;
                });
            });
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            var collection = GetCollection(headers.SchemaId());

            return collection.UpdateManyAsync(new BsonDocument(), Update.Unset(new StringFieldDefinition<MongoContentEntity>($"Data.{@event.FieldId}")));
        }

        protected Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var collection = GetCollection(headers.AggregateId());

            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.IsPublished).Text(x => x.Text));
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
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

        private async Task ForSchemaAsync(Guid schemaId, Func<IMongoCollection<MongoContentEntity>, Task> action)
        {
            var collection = GetCollection(schemaId);

            await action(collection);
        }

        private IMongoCollection<MongoContentEntity> GetCollection(Guid schemaId)
        {
            var name = $"{Prefix}{schemaId}";

            return database.GetCollection<MongoContentEntity>(name);
        }
    }
}
