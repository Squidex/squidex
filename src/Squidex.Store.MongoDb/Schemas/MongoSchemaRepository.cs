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
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Schemas.Repositories;
using Squidex.Store.MongoDb.Utils;

namespace Squidex.Store.MongoDb.Schemas
{
    public sealed class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, ICatchEventConsumer
    {
        private readonly SchemaJsonSerializer serializer;
        private readonly FieldRegistry fieldRegistry;

        public MongoSchemaRepository(IMongoDatabase database, SchemaJsonSerializer serializer, FieldRegistry fieldRegistry)
            : base(database)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.serializer = serializer;

            this.fieldRegistry = fieldRegistry;
        }

        protected override string CollectionName()
        {
            return "Projections_Schemas";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        public async Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId)
        {
            var entities = await Collection.Find(s => s.AppId == appId && !s.IsDeleted).ToListAsync();

            return entities.OfType<ISchemaEntity>().ToList();
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

        public Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, e => e.IsDeleted = true);
        }

        public Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.DeleteField(@event.FieldId));
        }

        public Task On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.DisableField(@event.FieldId));
        }

        public Task On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.EnableField(@event.FieldId));
        }

        public Task On(FieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.HideField(@event.FieldId));
        }

        public Task On(FieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.ShowField(@event.FieldId));
        }

        public Task On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.UpdateField(@event.FieldId, @event.Properties));
        }

        public Task On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.Update(@event.Properties));
        }

        public Task On(FieldAdded @event, EnvelopeHeaders headers)
        {
            var field = fieldRegistry.CreateField(@event.FieldId, @event.Name, @event.Properties);

            return UpdateSchema(headers, s => s.AddOrUpdateField(field));
        }

        public Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var schema = Schema.Create(@event.Name, @event.Properties);

            return Collection.CreateAsync(headers, s => { Serialize(s, schema); SimpleMapper.Map(@event, s); });
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
            
            Serialize(entity, currentSchema);
        }

        private void Serialize(MongoSchemaEntity entity, Schema schema)
        {
            entity.Schema = serializer.Serialize(schema).ToBsonDocument();
        }

        private Schema Deserialize(MongoSchemaEntity entity)
        {
            return entity.DeserializeSchema(serializer).Value;
        }
    }
}
