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
using PinkParrot.Infrastructure.Tasks;
using PinkParrot.Read.Models;

namespace PinkParrot.Read.Repositories.Implementations
{
    public sealed class MongoModelSchemaListRepository : MongoRepositoryBase<ModelSchemaListRM>, IModelSchemaRepository, ICatchEventConsumer
    {
        public MongoModelSchemaListRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<ModelSchemaListRM> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Id));
        }

        public IQueryable<ModelSchemaListRM> QuerySchemas()
        {
            return Collection.AsQueryable();
        }

        public Task<List<ModelSchemaListRM>> QueryAllAsync(Guid tenantId)
        {
            return Collection.Find(s => s.TenantId == tenantId && s.IsDeleted == false).ToListAsync();
        }

        public void On(ModelSchemaUpdated @event, EnvelopeHeaders headers)
        {
            Collection.UpdateAsync(headers, e => e.Name = @event.Properties.Name).Forget();
        }

        public void On(ModelSchemaDeleted @event, EnvelopeHeaders headers)
        {
            Collection.UpdateAsync(headers, e => e.IsDeleted = true).Forget();
        }

        public void On(ModelSchemaCreated @event, EnvelopeHeaders headers)
        {
            Collection.CreateAsync(headers, e => e.Name = @event.Properties.Name);
        }

        public void On(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload, @event.Headers);
        }
    }
}
