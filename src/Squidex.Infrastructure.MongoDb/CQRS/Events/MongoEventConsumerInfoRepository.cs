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
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class MongoEventConsumerInfoRepository : MongoRepositoryBase<MongoEventConsumerInfo>, IEventConsumerInfoRepository
    {
        private static readonly FieldDefinition<MongoEventConsumerInfo, string> NameField = Fields.Build(x => x.Name);
        private static readonly FieldDefinition<MongoEventConsumerInfo, string> ErrorField = Fields.Build(x => x.Error);
        private static readonly FieldDefinition<MongoEventConsumerInfo, string> PositionField = Fields.Build(x => x.Position);
        private static readonly FieldDefinition<MongoEventConsumerInfo, bool> IsStoppedField = Fields.Build(x => x.IsStopped);
        private static readonly FieldDefinition<MongoEventConsumerInfo, bool> IsResettingField = Fields.Build(x => x.IsResetting);

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
            var entity = await Collection.Find(Filter.Eq(NameField, consumerName)).FirstOrDefaultAsync();

            return entity;
        }

        public async Task CreateAsync(string consumerName)
        {
            if (await Collection.CountAsync(Filter.Eq(NameField, consumerName)) == 0)
            {
                try
                {
                    await Collection.InsertOneAsync(CreateEntity(consumerName, null));
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
            var filter = Filter.Eq(NameField, consumerName);

            return Collection.UpdateOneAsync(filter, Update.Unset(IsStoppedField).Unset(ErrorField));
        }

        public Task StopAsync(string consumerName, string error = null)
        {
            var filter = Filter.Eq(NameField, consumerName);

            return Collection.UpdateOneAsync(filter, Update.Set(IsStoppedField, true).Set(ErrorField, error));
        }

        public Task ResetAsync(string consumerName)
        {
            var filter = Filter.Eq(NameField, consumerName);

            return Collection.UpdateOneAsync(filter, Update.Set(IsResettingField, true).Unset(ErrorField));
        }

        public Task SetPositionAsync(string consumerName, string position, bool reset)
        {
            var filter = Filter.Eq(NameField, consumerName);

            if (reset)
            {
                return Collection.ReplaceOneAsync(filter, CreateEntity(consumerName, position));
            }
            else
            {
                return Collection.UpdateOneAsync(filter, Update.Set(PositionField, position));
            }
        }

        private static MongoEventConsumerInfo CreateEntity(string consumerName, string position)
        {
            return new MongoEventConsumerInfo { Name = consumerName, Position = position };
        }
    }
}
