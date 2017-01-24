// ==========================================================================
//  MongoSchemaRepository.cs
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
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Events.Schemas;
using Squidex.Events.Schemas.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Replay;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.MongoDb.Utils;

namespace Squidex.Read.MongoDb.Schemas
{
    public  class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, ICatchEventConsumer, IReplayableStore
    {
        private readonly SchemaJsonSerializer serializer;
        private readonly FieldRegistry registry;

        public MongoSchemaRepository(IMongoDatabase database, SchemaJsonSerializer serializer, FieldRegistry registry)
            : base(database)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(registry, nameof(registry));

            this.serializer = serializer;

            this.registry = registry;
        }

        protected override string CollectionName()
        {
            return "Projections_Schemas";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        public Task ClearAsync()
        {
            return TryDropCollectionAsync();
        }

        public async Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId)
        {
            var entities = await Collection.Find(s => s.AppId == appId && !s.IsDeleted).ToListAsync();

            return entities.OfType<ISchemaEntity>().ToList();
        }

        public async Task<IReadOnlyList<ISchemaEntityWithSchema>> QueryAllWithSchemaAsync(Guid appId)
        {
            var entities = await Collection.Find(s => s.AppId == appId && !s.IsDeleted).ToListAsync();

            entities.ForEach(x => x.DeserializeSchema(serializer));

            return entities.OfType<ISchemaEntityWithSchema>().ToList();
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid appId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name && s.AppId == appId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            entity?.DeserializeSchema(serializer);

            return entity;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid schemaId)
        {
            var entity = 
                await Collection.Find(s => s.Id == schemaId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            entity?.DeserializeSchema(serializer);

            return entity;
        }

        public async Task<Guid?> FindSchemaIdAsync(Guid appId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name & s.AppId == appId && !s.IsDeleted)
                    .Project<MongoSchemaEntity>(Projection.Include(x => x.Id)).FirstOrDefaultAsync();

            return entity?.Id;
        }

        protected Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, s => s.IsDeleted = true);
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldAdded @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s, registry));
        }

        protected Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var schema = Schema.Create(@event.Name, @event.Properties);

            return Collection.CreateAsync(headers, s => { UpdateSchema(s, schema); SimpleMapper.Map(@event, s); });
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        private Task UpdateSchema(EnvelopeHeaders headers, Func<Schema, Schema> updater)
        {
            return Collection.UpdateAsync(headers, e => UpdateSchema(e, updater));
        }

        private void UpdateSchema(MongoSchemaEntity entity, Func<Schema, Schema> updater)
        {
            var currentSchema = Deserialize(entity);

            currentSchema = updater(currentSchema);
            
            UpdateSchema(entity, currentSchema);
            UpdateProperties(entity, currentSchema);
        }

        private static void UpdateProperties(MongoSchemaEntity entity, Schema currentSchema)
        {
            entity.Label = currentSchema.Properties.Label;

            entity.IsPublished = currentSchema.IsPublished;
        }

        private void UpdateSchema(MongoSchemaEntity entity, Schema schema)
        {
            entity.Schema = serializer.Serialize(schema).ToString();
        }

        private Schema Deserialize(MongoSchemaEntity entity)
        {
            return entity.DeserializeSchema(serializer).Value;
        }
    }
}
