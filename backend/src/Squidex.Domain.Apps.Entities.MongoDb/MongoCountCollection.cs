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
    public sealed class MongoCountCollection : MongoRepositoryBase<MongoCountEntity>
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
            var update = Update.Inc(x => x.Count, isDeleted ? -1 : 1);

            return Collection.UpdateOneAsync(x => x.Key == key, update, Upsert);
        }

        public Task UpdateAsync(IEnumerable<DomainId> keys, bool isDeleted = false)
        {
            return UpdateAsync(keys.Select(x => x.ToString()), isDeleted);
        }

        public Task UpdateAsync(IEnumerable<string> keys, bool isDeleted = false)
        {
            var update = Update.Inc(x => x.Count, isDeleted ? -1 : 1);

            var writes =
                keys.GroupBy(x => x).Select(x =>
                    new UpdateOneModel<MongoCountEntity>(
                        Filter.Eq(x => x.Key, x.Key), Update.Inc(x => x.Count, isDeleted ? -x.Count() : x.Count()))
                        {
                            IsUpsert = true
                        }).ToList();

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered);
        }
    }
}
