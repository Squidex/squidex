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
using Squidex.Events.Contents;
using Squidex.Events.Schemas;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.MongoDb.Utils;

// ReSharper disable UnusedParameterGlobal
// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Read.MongoDb.Contents
{
    public partial class MongoContentRepository
    {
        private static UpdateDefinitionBuilder<MongoContentEntity> Update
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
            return ForSchemaIdAsync(@event.SchemaId.Id, async collection =>
            {
                await collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.IsPublished));
                await collection.Indexes.CreateOneAsync(IndexKeys.Text(x => x.Text));
            });
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.SchemaId.Id, (collection, schema) =>
            {
                return collection.CreateAsync(@event, headers, x =>
                {
                    SimpleMapper.Map(@event, x);

                    x.SetData(schema, @event.Data);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.SchemaId.Id, (collection, schema) =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.SetData(schema, @event.Data);
                });
            });
        }

        protected Task On(ContentPublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(@event.SchemaId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.IsPublished = true;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(@event.SchemaId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.IsPublished = false;
                });
            });
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(@event.SchemaId.Id, collection =>
            {
                return collection.UpdateManyAsync(new BsonDocument(), Update.Unset(new StringFieldDefinition<MongoContentEntity>($"Data.{@event.FieldId}")));
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return ForSchemaIdAsync(@event.SchemaId.Id, collection =>
            {
                return collection.DeleteOneAsync(x => x.Id == headers.AggregateId());
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
