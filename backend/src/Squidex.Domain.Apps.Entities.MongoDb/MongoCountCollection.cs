// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb
{
    internal sealed class MongoCountCollection : MongoRepositoryBase<MongoCountEntity>
    {
        private readonly string name;

        public MongoCountCollection(IMongoDatabase database, string name)
            : base(database)
        {
            this.name = name;

            InitializeAsync(default).Wait();
        }

        protected override string CollectionName()
        {
            return name;
        }

        public Task<long> CountAsync(DomainId key,
            CancellationToken ct)
        {
            return CountAsync(key.ToString(), ct);
        }

        public async Task<long> CountAsync(string key,
            CancellationToken ct)
        {
            var entity = await Collection.Find(x => x.Key == key).FirstOrDefaultAsync(ct);

            return entity?.Count ?? 0L;
        }

        public Task UpdateAsync(DomainId key, PersistenceAction action,
            CancellationToken ct = default)
        {
            return UpdateAsync(key.ToString(), action, ct);
        }

        public Task UpdateAsync(string key, PersistenceAction action,
            CancellationToken ct = default)
        {
            if (action == PersistenceAction.Undefined || action == PersistenceAction.Update)
            {
                return Task.CompletedTask;
            }

            var update = Update.Inc(x => x.Count, Inc(action));

            return Collection.UpdateOneAsync(x => x.Key == key, update, Upsert, ct);
        }

        public Task UpdateAsync(IEnumerable<(DomainId Key, PersistenceAction Action)> values,
            CancellationToken ct = default)
        {
            return UpdateAsync(values.Select(x => (x.Key.ToString(), x.Action)), ct);
        }

        public Task UpdateAsync(IEnumerable<(string Key, PersistenceAction Action)> values,
            CancellationToken ct = default)
        {
            var writes = values.GroupBy(x => x.Key).Select(CreateUpdate).NotNull().ToList();

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }

        public Task SetAsync(IEnumerable<(DomainId Key, long Value)> values,
            CancellationToken ct = default)
        {
            return SetAsync(values.Select(x => (x.Key.ToString(), x.Value)), ct);
        }

        public Task SetAsync(IEnumerable<(string Key, long Value)> values,
            CancellationToken ct = default)
        {
            var writes = values.Select(CreateUpdate).NotNull().ToList();

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

        private static UpdateOneModel<MongoCountEntity>? CreateUpdate(IGrouping<string, (string Key, PersistenceAction Action)> group)
        {
            var inc = group.Sum(x => Inc(x.Action));

            if (inc == 0)
            {
                return null;
            }

            var update = Update.Inc(y => y.Count, inc);

            return new UpdateOneModel<MongoCountEntity>(Filter.Eq(x => x.Key, group.Key), update)
            {
                IsUpsert = true
            };
        }

        private static int Inc(PersistenceAction action)
        {
            switch (action)
            {
                case PersistenceAction.Create:
                    return 1;
                case PersistenceAction.Delete:
                    return -1;
                default:
                    return 0;
            }
        }
    }
}
