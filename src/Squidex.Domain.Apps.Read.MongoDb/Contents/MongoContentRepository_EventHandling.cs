// ==========================================================================
//  MongoContentRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

// ReSharper disable UnusedParameterGlobal
// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public partial class MongoContentRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(content-)|(schema-)|(asset-)"; }
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
            return ForAppIdAsync(@event.AppId.Id, async collection =>
            {
                await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ReferencedIds));
                await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.IsPublished));
                await collection.Indexes.CreateOneAsync(Index.Text(x => x.DataText));
            });
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schemaEntity) =>
            {
                return collection.CreateAsync(@event, headers, x =>
                {
                    x.SchemaId = @event.SchemaId.Id;

                    SimpleMapper.Map(@event, x);

                    x.SetData(schemaEntity.Schema, @event.Data);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schemaEntity) =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.SetData(schemaEntity.Schema, @event.Data);
                });
            });
        }

        protected Task On(ContentPublished @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.IsPublished = true;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.IsPublished = false;
                });
            });
        }

        protected Task On(AssetDeleted @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateManyAsync(
                    Filter.And(
                        Filter.AnyEq(x => x.ReferencedIds, @event.AssetId),
                        Filter.AnyNe(x => x.ReferencedIdsDeleted, @event.AssetId)),
                    Update.AddToSet(x => x.ReferencedIdsDeleted, @event.AssetId));
            });
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, async collection =>
            {
                await collection.UpdateManyAsync(
                    Filter.And(
                        Filter.AnyEq(x => x.ReferencedIds, @event.ContentId),
                        Filter.AnyNe(x => x.ReferencedIdsDeleted, @event.ContentId)),
                    Update.AddToSet(x => x.ReferencedIdsDeleted, @event.ContentId));

                await collection.DeleteOneAsync(x => x.Id == headers.AggregateId());
            });
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
