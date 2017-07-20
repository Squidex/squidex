using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis.EventStore
{
    public class RedisEventStore : IEventStore, IExternalSystem
    {
        private readonly Lazy<IConnectionMultiplexer> redisClient;

        public RedisEventStore(Lazy<IConnectionMultiplexer> redis)
        {
            this.redisClient = redis;
        }

        public Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName)
        {
            var db = redisClient.Value.GetDatabase();

            
            throw new NotImplementedException();
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
        {
            throw new NotImplementedException();
        }

        public IEventSubscription CreateSubscription(string streamFilter = null, string position = null)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            try
            {
                redisClient.Value.GetStatus();

                var db = redisClient.Value.GetDatabase();
                if (!db.HashExists("SquidexEventStore", 0))
                {
                    db.HashSet("SquidexEventStore", new[] { new HashEntry(0, "{event data}") });
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Redis connection failed to connect to database {redisClient.Value.Configuration}", ex);
            }
        }
    }
}
