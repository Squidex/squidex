// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Users.MongoDb.Infrastructure
{
    public class MongoPersistedGrantStore : MongoRepositoryBase<PersistedGrant>, IPersistedGrantStore
    {
        static MongoPersistedGrantStore()
        {
            BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
            {
                cm.AutoMap();

                cm.MapIdProperty(x => x.Key);
            });
        }

        public MongoPersistedGrantStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_PersistedGrants";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<PersistedGrant> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<PersistedGrant>(Index.Ascending(x => x.ClientId)),
                new CreateIndexModel<PersistedGrant>(Index.Ascending(x => x.SubjectId))
            }, ct);
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return await Collection.Find(x => x.SubjectId == subjectId).ToListAsync();
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            return await Collection.Find(CreateFilter(filter)).ToListAsync();
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return Collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            return Collection.DeleteManyAsync(CreateFilter(filter));
        }

        public Task RemoveAsync(string key)
        {
            return Collection.DeleteManyAsync(x => x.Key == key);
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            return Collection.ReplaceOneAsync(x => x.Key == grant.Key, grant, UpsertReplace);
        }

        private static FilterDefinition<PersistedGrant> CreateFilter(PersistedGrantFilter filter)
        {
            var filters = new List<FilterDefinition<PersistedGrant>>();

            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                filters.Add(Filter.Eq(x => x.ClientId, filter.ClientId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                filters.Add(Filter.Eq(x => x.SessionId, filter.SessionId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                filters.Add(Filter.Eq(x => x.SubjectId, filter.SubjectId));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                filters.Add(Filter.Eq(x => x.Type, filter.Type));
            }

            if (filters.Count > 0)
            {
                return Filter.And(filters);
            }

            return new BsonDocument();
        }
    }
}
