// ==========================================================================
//  MongoEventConsumerInfoRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class MongoEventConsumerInfoRepository : MongoRepositoryBase<MongoEventConsumerInfo>, IEventConsumerInfoRepository
    {
        public MongoEventConsumerInfoRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "EventPositions";
        }

        public async Task<IReadOnlyList<IEventConsumerInfo>> QueryAsync()
        {
            var entities = await Collection.Find(new BsonDocument()).SortBy(x => x.Name).ToListAsync();

            return entities.OfType<IEventConsumerInfo>().ToList();
        }

        public async Task<IEventConsumerInfo> FindAsync(string consumerName)
        {
            var entity = await Collection.Find(x => x.Name == consumerName).FirstOrDefaultAsync();

            return entity;
        }

        public async Task CreateAsync(string consumerName)
        {
            if (await Collection.CountAsync(x => x.Name == consumerName) == 0)
            {
                try
                {
                    await Collection.InsertOneAsync(new MongoEventConsumerInfo { Name = consumerName, Position = null });
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError?.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }
        }

        public Task StartAsync(string consumerName)
        {
            return Collection.UpdateOneAsync(x => x.Name == consumerName, Update.Unset(x => x.IsStopped));
        }

        public Task StopAsync(string consumerName, string error = null)
        {
            return Collection.UpdateOneAsync(x => x.Name == consumerName, Update.Set(x => x.IsStopped, true).Set(x => x.Error, error));
        }

        public Task ResetAsync(string consumerName)
        {
            return Collection.UpdateOneAsync(x => x.Name == consumerName, Update.Set(x => x.IsResetting, true));
        }

        public Task SetLastHandledEventNumberAsync(string consumerName, string position)
        {
            return Collection.ReplaceOneAsync(x => x.Name == consumerName, CreateEntity(consumerName, position));
        }

        private static MongoEventConsumerInfo CreateEntity(string consumerName, string position)
        {
            return new MongoEventConsumerInfo { Name = consumerName, Position = position };
        }
    }
}
