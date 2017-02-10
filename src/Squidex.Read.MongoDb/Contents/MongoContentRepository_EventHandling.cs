// ==========================================================================
//  MongoContentRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Events;
using Squidex.Events.Contents;
using Squidex.Events.Schemas;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.MongoDb.Utils;

// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Read.MongoDb.Contents
{
    public partial class MongoContentRepository
    {
        protected UpdateDefinitionBuilder<MongoContentEntity> Update
        {
            get
            {
                return Builders<MongoContentEntity>.Update;
            }
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

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(headers.AggregateId(), async collection =>
            {
                await collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.IsPublished));
                await collection.Indexes.CreateOneAsync(IndexKeys.Text(x => x.Text));
            });
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
            return ForSchemaIdAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = true;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsPublished = false;
                });
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateAsync(headers, x =>
                {
                    x.IsDeleted = true;
                });
            });
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(headers.SchemaId(), collection =>
            {
                return collection.UpdateManyAsync(new BsonDocument(), Update.Unset(new StringFieldDefinition<MongoContentEntity>($"Data.{@event.FieldId}")));
            });
        }

        private async Task ForSchemaIdAsync(Guid schemaId, Func<IMongoCollection<MongoContentEntity>, Task> action)
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
