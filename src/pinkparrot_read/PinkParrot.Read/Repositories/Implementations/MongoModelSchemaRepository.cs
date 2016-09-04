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
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.MongoDb;
using PinkParrot.Read.Models;

namespace PinkParrot.Read.Repositories.Implementations
{
    public sealed class MongoModelSchemaRepository : BaseMongoDbRepository<ModelSchemaRM>, IModelSchemaRepository, ICatchEventConsumer
    {
        public MongoModelSchemaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<ModelSchemaRM> collection)
        {
            return Collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.SchemaId));
        }

        public IQueryable<ModelSchemaRM> QuerySchemas()
        {
            return Collection.AsQueryable();
        }

        public Task<List<ModelSchemaRM>> QueryAllAsync()
        {
            return Collection.Find(s => true).ToListAsync();
        }

        public async void On(ModelSchemaCreated @event, EnvelopeHeaders headers)
        {
            var now = DateTime.UtcNow;

            var entity = new ModelSchemaRM
            {
                SchemaId = headers.AggregateId(),
                Created = now,
                Modified = now,
                Name = @event.Properties.Name,
                Hints = @event.Properties.Hints,
                Label = @event.Properties.Label,
            };

            await Collection.InsertOneAsync(entity);
        }

        public void On(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload, @event.Headers);
        }
    }
}
