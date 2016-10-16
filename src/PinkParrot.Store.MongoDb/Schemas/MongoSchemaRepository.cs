// ==========================================================================
//  MongoSchemaRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using PinkParrot.Core.Schemas;
using PinkParrot.Events.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Read.Schemas.Repositories;
using PinkParrot.Store.MongoDb.Schemas.Models;
using PinkParrot.Store.MongoDb.Utils;

namespace PinkParrot.Store.MongoDb.Schemas
{
    public sealed class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, ICatchEventConsumer
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly FieldRegistry fieldRegistry;

        public MongoSchemaRepository(IMongoDatabase database, JsonSerializerSettings serializerSettings, FieldRegistry fieldRegistry)
            : base(database)
        {
            Guard.NotNull(serializerSettings, nameof(serializerSettings));
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.serializerSettings = serializerSettings;
            this.fieldRegistry = fieldRegistry;
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

            entity?.DeserializeSchema(serializerSettings, fieldRegistry);

            return entity;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid schemaId)
        {
            var entity = 
                await Collection.Find(s => s.Id == schemaId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            entity?.DeserializeSchema(serializerSettings, fieldRegistry);

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
            return Collection.CreateAsync(headers, s => SimpleMapper.Map(@event, s));
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        private void UpdateSchema(MongoSchemaEntity entity, Func<Schema, Schema> updater)
        {
            var currentSchema = Deserialize(entity);

            currentSchema = updater(currentSchema);
            
            Serialize(entity, currentSchema);
        }

        private Task UpdateSchema(EnvelopeHeaders headers, Func<Schema, Schema> updater)
        {
            return Collection.UpdateAsync(headers, e=> UpdateSchema(e, updater));
        }

        private void Serialize(MongoSchemaEntity entity, Schema schema)
        {
            var dto = SchemaDto.Create(schema);

            entity.Schema = dto.ToJsonBsonDocument(serializerSettings);
        }

        private Schema Deserialize(MongoSchemaEntity entity)
        {
            var dto = entity?.Schema.ToJsonObject<SchemaDto>(serializerSettings);

            return dto?.ToSchema(fieldRegistry);
        }
    }
}
