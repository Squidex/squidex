// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb
{
    internal sealed class MongoCountCollection : MongoRepositoryBase<MongoCountEntity>
    {
        private readonly string name;

        public MongoCountCollection(IMongoDatabase database, string name)
            : base(database)
        {
            this.name = name;

            InitializeAsync().Wait();
        }

        protected override string CollectionName()
        {
            return name;
        }

        public Task<long> CountAsync(DomainId key, CancellationToken ct)
        {
            return CountAsync(key.ToString(), ct);
        }

        public async Task<long> CountAsync(string key, CancellationToken ct)
        {
            var entity = await Collection.Find(x => x.Key == key).FirstOrDefaultAsync(ct);

            return entity?.Count ?? 0L;
        }

        public Task UpdateAsync(DomainId key, bool isDeleted = false)
        {
            return UpdateAsync(key.ToString(), isDeleted);
        }

        public Task UpdateAsync(string key, bool isDeleted = false)
        {
            var update = Update.Inc(x => x.Count, Inc(isDeleted));

            return Collection.UpdateOneAsync(x => x.Key == key, update, Upsert);
        }

        public Task UpdateAsync(IEnumerable<(DomainId Key, bool IsDeleted)> values, CancellationToken ct = default)
        {
            return UpdateAsync(values.Select(x => (x.Key.ToString(), x.IsDeleted)), ct);
        }

        public Task UpdateAsync(IEnumerable<(string Key, bool IsDeleted)> values, CancellationToken ct = default)
        {
            var writes = values.GroupBy(x => x.Key).Select(CreateUpdate).ToList();

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }

        public Task SetAsync(IEnumerable<(DomainId Key, long Value)> values, CancellationToken ct = default)
        {
            return SetAsync(values.Select(x => (x.Key.ToString(), x.Value)), ct);
        }

        public Task SetAsync(IEnumerable<(string Key, long Value)> values, CancellationToken ct = default)
        {
            var writes = values.Select(CreateUpdate).ToList();

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }

        private static UpdateOneModel<MongoCountEntity> CreateUpdate((string Key, long Value) value)
        {
            var update = Update.Set(y => y.Count, value.Value);

            return new UpdateOneModel<MongoCountEntity>(Filter.Eq(x => x.Key, value.Key), update)
            {
                IsUpsert = true
            };
        }

        private static UpdateOneModel<MongoCountEntity> CreateUpdate(IGrouping<string, (string Key, bool IsDeleted)> group)
        {
            var update = Update.Inc(y => y.Count, group.Sum(x => Inc(x.IsDeleted)));

            return new UpdateOneModel<MongoCountEntity>(Filter.Eq(x => x.Key, group.Key), update)
            {
                IsUpsert = true
            };
        }

        private static int Inc(bool isDeleted)
        {
            return isDeleted ? -1 : 1;
        }
    }
}
