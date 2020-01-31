﻿// ==========================================================================
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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Users.MongoDb.Infrastructure
{
    public class MongoPersistedGrantStore : MongoRepositoryBase<PersistedGrant>, IPersistedGrantStore
    {
        static MongoPersistedGrantStore()
        {
            BsonClassMap.RegisterClassMap<PersistedGrant>(map =>
            {
                map.AutoMap();

                map.MapIdProperty(x => x.Key).SetSerializer(new StringSerializer(BsonType.ObjectId));
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

        public Task StoreAsync(PersistedGrant grant)
        {
            return Collection.ReplaceOneAsync(x => x.Key == grant.Key, grant, UpsertReplace);
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return Collection.Find(x => x.SubjectId == subjectId).ToListAsync().ContinueWith(x => (IEnumerable<PersistedGrant>)x.Result);
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return Collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            return Collection.DeleteManyAsync(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type);
        }

        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            return Collection.DeleteManyAsync(x => x.SubjectId == subjectId && x.ClientId == clientId);
        }

        public Task RemoveAsync(string key)
        {
            return Collection.DeleteManyAsync(x => x.Key == key);
        }
    }
}
