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

        public Task ClearAsync(IEnumerable<string> currentConsumerNames)
        {
            return Collection.DeleteManyAsync(Filter.Not(Filter.In(NameField, currentConsumerNames)));
        }

        public async Task SetAsync(string consumerName, string position, bool isStopped = false, string error = null)
        {
            try
            {
                await Collection.UpdateOneAsync(Filter.Eq(NameField, consumerName),
                    Update
                        .Set(ErrorField, error)
                        .Set(PositionField, position)
                        .Set(IsStoppedField, isStopped),
                new UpdateOptions { IsUpsert = true });
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
}
