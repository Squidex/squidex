// ==========================================================================
//  MongoDbStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Read.MongoDb.Utils
{
    public sealed class EventPosition
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement]
        [BsonRequired]
        public long EventNumber { get; set; }
    }

    public sealed class MongoDbConsumerWrapper : MongoRepositoryBase<EventPosition>, IEventCatchConsumer
    {
        private static readonly UpdateOptions upsert = new UpdateOptions { IsUpsert = true };
        private readonly IEventConsumer eventConsumer;
        private readonly string eventStoreName;

        public MongoDbConsumerWrapper(IMongoDatabase database, IEventConsumer eventConsumer) 
            : base(database)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            this.eventConsumer = eventConsumer;

            eventStoreName = eventConsumer.GetType().Name;
        }

        protected override string CollectionName()
        {
            return "EventPositions";
        }

        public async Task On(Envelope<IEvent> @event, long eventNumber)
        {
            await eventConsumer.On(@event);

            await SetLastHandledEventNumber(eventNumber);
        }

        private Task SetLastHandledEventNumber(long eventNumber)
        {
            return Collection.ReplaceOneAsync(x => x.Name == eventStoreName, new EventPosition { Name = eventStoreName, EventNumber = eventNumber }, upsert);
        }

        public async Task<long> GetLastHandledEventNumber()
        {
            var collectionPosition =
                await Collection
                    .Find(x => x.Name == eventStoreName).SortByDescending(x => x.EventNumber).Limit(1)
                    .FirstOrDefaultAsync();
                    
            return collectionPosition?.EventNumber ?? -1;
        }
    }
}
