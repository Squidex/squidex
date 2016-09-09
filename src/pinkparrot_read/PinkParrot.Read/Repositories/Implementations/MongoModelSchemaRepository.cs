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
using PinkParrot.Core.Schema.Json;
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.MongoDb;

namespace PinkParrot.Read.Repositories.Implementations
{
    public sealed class MongoModelSchemaRepository : MongoRepositoryBase<MongoModelSchemaEntity>, IModelSchemaRepository, ICatchEventConsumer
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly ModelFieldFactory factory;

        public MongoModelSchemaRepository(IMongoDatabase database, JsonSerializerSettings serializerSettings, ModelFieldFactory factory)
            : base(database)
        {
            Guard.NotNull(serializerSettings, nameof(serializerSettings));
            Guard.NotNull(factory, nameof(factory));

            this.serializerSettings = serializerSettings;

            this.factory = factory;
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

        public Task On(ModelFieldAdded @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => s.AddField(@event.FieldId, @event.Properties, factory));
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
            return UpdateSchema(headers, s => s.SetField(@event.FieldId, @event.Properties));
        }

        public Task On(ModelSchemaUpdated @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, e =>
            {
                if (!string.IsNullOrWhiteSpace(@event.Properties.Name))
                {
                    e.Name = @event.Properties.Name;
                }

                UpdateSchema(e, s => s.Update(@event.Properties));
            });
        }

        public Task On(ModelSchemaCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, e =>
            {
                e.Name = @event.Properties.Name;

                Serialize(e, ModelSchema.Create(@event.Properties));
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
            entity.Schema = SchemaDto.Create(schema).ToJsonBsonDocument(serializerSettings);
        }

        private ModelSchema Deserialize(MongoModelSchemaEntity entity)
        {
            return entity?.Schema.ToJsonObject<SchemaDto>(serializerSettings).ToModelSchema(factory);
        }
    }
}
