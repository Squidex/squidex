// ==========================================================================
//  MongoModelSchemaRepository.cs
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
using PinkParrot.Core.Schema;
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.MongoDb;
using PinkParrot.Read.Models;

namespace PinkParrot.Read.Repositories.Implementations.Mongo
{
    public sealed class MongoModelSchemaRepository : MongoRepositoryBase<MongoModelSchemaEntity>, IModelSchemaRepository, ICatchEventConsumer
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly ModelFieldRegistry fieldRegistry;

        public MongoModelSchemaRepository(IMongoDatabase database, JsonSerializerSettings serializerSettings, ModelFieldRegistry fieldRegistry)
            : base(database)
        {
            Guard.NotNull(serializerSettings, nameof(serializerSettings));
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.serializerSettings = serializerSettings;
            this.fieldRegistry = fieldRegistry;
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoModelSchemaEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        public async Task<List<IModelSchemaEntity>> QueryAllAsync(Guid tenantId)
        {
            var entities = await Collection.Find(s => s.TenantId == tenantId && !s.IsDeleted).ToListAsync();

            return entities.OfType<IModelSchemaEntity>().ToList();
        }

        public async Task<EntityWithSchema> FindSchemaAsync(Guid tenantId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name && s.TenantId == tenantId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            return entity != null ? new EntityWithSchema(entity, Deserialize(entity)) : null;
        }

        public async Task<EntityWithSchema> FindSchemaAsync(Guid schemaId)
        {
            var entity = 
                await Collection.Find(s => s.Id == schemaId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            return entity != null ? new EntityWithSchema(entity, Deserialize(entity)) : null;
        }

        public async Task<Guid?> FindSchemaIdAsync(Guid tenantId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name & s.TenantId == tenantId && !s.IsDeleted)
                    .Project<MongoModelSchemaEntity>(Projection.Include(x => x.Id)).FirstOrDefaultAsync();

            return entity?.Id;
        }

        public Task On(ModelSchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, e => e.IsDeleted = true);
        }

        public Task On(ModelFieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.DeleteField(@event.FieldId));
        }

        public Task On(ModelFieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.DisableField(@event.FieldId));
        }

        public Task On(ModelFieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.EnableField(@event.FieldId));
        }

        public Task On(ModelFieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.HideField(@event.FieldId));
        }

        public Task On(ModelFieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.ShowField(@event.FieldId));
        }

        public Task On(ModelFieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.UpdateField(@event.FieldId, @event.Properties));
        }

        public Task On(ModelSchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.Update(@event.Properties));
        }

        public Task On(ModelFieldAdded @event, EnvelopeHeaders headers)
        {
            var field = fieldRegistry.CreateField(@event.FieldId, @event.Name, @event.Properties);

            return UpdateSchema(headers, s => s.AddOrUpdateField(field));
        }

        public Task On(ModelSchemaCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, e =>
            {
                e.Name = @event.Name;

                Serialize(e, ModelSchema.Create(@event.Name, @event.Properties));
            });
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        private void UpdateSchema(MongoModelSchemaEntity entity, Func<ModelSchema, ModelSchema> updater)
        {
            var currentSchema = Deserialize(entity);

            currentSchema = updater(currentSchema);
            
            Serialize(entity, currentSchema);
        }

        private Task UpdateSchema(EnvelopeHeaders headers, Func<ModelSchema, ModelSchema> updater)
        {
            return Collection.UpdateAsync(headers, e=> UpdateSchema(e, updater));
        }

        private void Serialize(MongoModelSchemaEntity entity, ModelSchema schema)
        {
            var dto = ModelSchemaDto.Create(schema);

            entity.Schema = dto.ToJsonBsonDocument(serializerSettings);
        }

        private ModelSchema Deserialize(MongoModelSchemaEntity entity)
        {
            var dto = entity?.Schema.ToJsonObject<ModelSchemaDto>(serializerSettings);

            return dto?.ToSchema(fieldRegistry);
        }
    }
}
