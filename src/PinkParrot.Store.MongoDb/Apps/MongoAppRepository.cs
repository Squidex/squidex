// ==========================================================================
//  MongoAppRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PinkParrot.Events.Apps;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Read.Apps;
using PinkParrot.Read.Apps.Repositories;
using PinkParrot.Store.MongoDb.Utils;

namespace PinkParrot.Store.MongoDb.Apps
{
    public sealed class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository, ICatchEventConsumer
    {
        public MongoAppRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        public async Task<IReadOnlyList<IAppEntity>> QueryAllAsync()
        {
            var entities =
                await Collection.Find(new BsonDocument()).ToListAsync();

            return entities;
        }

        public async Task<IAppEntity> FindAppByNameAsync(string name)
        {
            var entity =
                await Collection.Find(s => s.Name == name).FirstOrDefaultAsync();

            return entity;
        }

        public Task On(AppCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, a => SimpleMapper.Map(a, @event));
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }
    }
}
