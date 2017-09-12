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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0612 // Type or member is obsolete

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
                return collection.CreateAsync(@event, headers, x =>
                {
                    x.SchemaId = @event.SchemaId.Id;

                    SimpleMapper.Map(@event, x);

                    x.SetData(schema.SchemaDef, @event.Data);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schema) =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.SetData(schema.SchemaDef, @event.Data);
                });
            });
        }

        protected Task On(ContentPublished @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.Status = Status.Published;
                });
            });
        }

        protected Task On(ContentUnpublished @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.Status = Status.Draft;
                });
            });
        }

        protected Task On(ContentArchived @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.Status = Status.Archived;
                });
            });
        }

        protected Task On(ContentStatusChanged @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.Status = @event.Status;
                });
            });
        }

        protected Task On(ContentRestored @event, EnvelopeHeaders headers)
        {
            return ForAppIdAsync(@event.AppId.Id, collection =>
            {
                return collection.UpdateAsync(@event, headers, x =>
                {
                    x.Status = Status.Draft;
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
