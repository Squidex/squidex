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
using MongoDB.Driver;
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
using Squidex.Store.MongoDb.Utils;

namespace Squidex.Store.MongoDb.Contents
{
    public class MongoContentRepository : IContentRepository, ICatchEventConsumer, IReplayableStore
    {
        private const string Prefix = "Projections_Content_";
        private readonly IMongoDatabase database;

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

        public MongoContentRepository(IMongoDatabase database)
        {
            Guard.NotNull(database, nameof(database));

            this.database = database;
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

            var entities = 
                await cursor.ToListAsync();

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

            var entity = 
                await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

            return entity;
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            var collection = GetCollection(headers.SchemaId());

            return collection.CreateAsync(headers, x =>
            {
                SimpleMapper.Map(@event, x);
                
                x.SetData(@event.Data);
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            var collection = GetCollection(headers.SchemaId());

            return collection.UpdateAsync(headers, x =>
            {
                x.SetData(@event.Data);
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            var collection = GetCollection(headers.SchemaId());

            return collection.UpdateAsync(headers, x =>
            {
                x.IsDeleted = true;
            });
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

        private IMongoCollection<MongoContentEntity> GetCollection(Guid schemaId)
        {
            var name = $"{Prefix}{schemaId}";

            return database.GetCollection<MongoContentEntity>(name);
        }
    }
}
