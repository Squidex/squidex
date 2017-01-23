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
using Squidex.Store.MongoDb.Utils;

namespace Squidex.Store.MongoDb.Contents
{
    public class MongoContentRepository : IContentRepository, ICatchEventConsumer, IReplayableStore
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;
        private readonly ISchemaProvider schemaProvider;

        protected ProjectionDefinitionBuilder<MongoContentEntity> Projection
        {
            get
            {
                return Builders<MongoContentEntity>.Projection;
            }
        }

        protected SortDefinitionBuilder<MongoContentEntity> Sort
        {
            get
            {
                return Builders<MongoContentEntity>.Sort;
            }
        }

        protected UpdateDefinitionBuilder<MongoContentEntity> Update
        {
            get
            {
                return Builders<MongoContentEntity>.Update;
            }
        }

        protected FilterDefinitionBuilder<MongoContentEntity> Filter
        {
            get
            {
                return Builders<MongoContentEntity>.Filter;
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

        public async Task<List<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, int? take, int? skip, string query)
        {
            var cursor = BuildQuery(schemaId, nonPublished, query);

            if (take.HasValue)
            {
                cursor.Limit(take.Value);
            }

            if (skip.HasValue)
            {
                cursor.Skip(skip.Value);
            }

            cursor.SortByDescending(x => x.LastModified);

            var schemaEntity = await schemaProvider.FindSchemaByIdAsync(schemaId);

            if (schemaEntity == null)
            {
                return new List<IContentEntity>();
            }

            var entities = await cursor.ToListAsync();

            foreach (var entity in entities)
            {
                entity.ParseData(schemaEntity.Schema);
            }

            return entities.OfType<IContentEntity>().ToList();
        }

        public Task<long> CountAsync(Guid schemaId, bool nonPublished, string query)
        {
            var cursor = BuildQuery(schemaId, nonPublished, query);

            return cursor.CountAsync();
        }

        private IFindFluent<MongoContentEntity, MongoContentEntity> BuildQuery(Guid schemaId, bool nonPublished, string query)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                filters.Add(Filter.Text(query, "en"));
            }

            if (!nonPublished)
            {
                filters.Add(Filter.Eq(x => x.IsPublished, false));
            }

            var collection = GetCollection(schemaId);

            var cursor = collection.Find(Filter.And(filters));

            return cursor;
        }

        public async Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id)
        {
            var collection = GetCollection(schemaId);

            var entity = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }

            var schemaEntity = await schemaProvider.FindSchemaByIdAsync(schemaId);

            if (schemaEntity == null)
            {
                return null;
            }

            entity.ParseData(schemaEntity.Schema);

            return entity;
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers, (collection, schema) =>
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
            return ForSchemaAsync(headers, (collection, schema) =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.SetData(schema, @event.Data);
                });
            });
        }

        protected Task On(ContentPublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers, collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = true;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers, collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = false;
                });
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(headers, collection =>
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

        private async Task ForSchemaAsync(EnvelopeHeaders headers, Func<IMongoCollection<MongoContentEntity>, Schema, Task> action)
        {
            var collection = GetCollection(headers.SchemaId());

            var schemaEntity = await schemaProvider.FindSchemaByIdAsync(headers.SchemaId());

            if (schemaEntity == null)
            {
                return;
            }

            await action(collection, schemaEntity.Schema);
        }

        private async Task ForSchemaAsync(EnvelopeHeaders headers, Func<IMongoCollection<MongoContentEntity>, Task> action)
        {
            var collection = GetCollection(headers.SchemaId());

            await action(collection);
        }

        private IMongoCollection<MongoContentEntity> GetCollection(Guid schemaId)
        {
            var name = $"{Prefix}{schemaId}";

            return database.GetCollection<MongoContentEntity>(name);
        }
    }
}
