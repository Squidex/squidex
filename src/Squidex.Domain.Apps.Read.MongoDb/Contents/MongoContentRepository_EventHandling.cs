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
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

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
            get { return "^(content-)|(app-)|(asset-)"; }
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

        protected Task On(AppCreated @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, async collection =>
            {
                await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.SchemaId).Descending(x => x.LastModified));
                await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ReferencedIds));
                await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Status));
                await collection.Indexes.CreateOneAsync(Index.Text(x => x.DataText));
            });
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schema) =>
            {
                return collection.CreateAsync(@event, headers, content =>
                {
                    content.SchemaId = @event.SchemaId.Id;

                    SimpleMapper.Map(@event, content);

                    var idData = @event.Data?.ToIdModel(schema.SchemaDef, true);

                    content.DataText = idData?.ToFullText();
                    content.DataDocument = idData?.ToBsonDocument(serializer);
                    content.ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schema) =>
            {
                var idData = @event.Data.ToIdModel(schema.SchemaDef, true);

                return collection.UpdateOneAsync(
                    Filter.Eq(x => x.Id, @event.ContentId),
                    Update
                        .Set(x => x.DataText, idData.ToFullText())
                        .Set(x => x.DataDocument, idData.ToBsonDocument(serializer))
                        .Set(x => x.ReferencedIds, idData.ToReferencedIds(schema.SchemaDef))
                        .Set(x => x.LastModified, headers.Timestamp())
                        .Set(x => x.LastModifiedBy, @event.Actor)
                        .Set(x => x.Version, headers.EventStreamNumber()));
            });
        }

        protected Task On(ContentStatusChanged @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateOneAsync(
                    Filter.Eq(x => x.Id, @event.ContentId),
                    Update
                        .Set(x => x.Status, @event.Status)
                        .Set(x => x.LastModified, headers.Timestamp())
                        .Set(x => x.LastModifiedBy, @event.Actor)
                        .Set(x => x.Version, headers.EventStreamNumber()));
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

                await collection.DeleteOneAsync(x => x.Id == @event.ContentId);
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
