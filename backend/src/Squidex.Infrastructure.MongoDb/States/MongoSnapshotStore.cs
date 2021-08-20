// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public class MongoSnapshotStore<T> : MongoRepositoryBase<MongoState<T>>, ISnapshotStore<T>
    {
        public MongoSnapshotStore(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, Register(jsonSerializer))
        {
        }

        private static bool Register(JsonSerializer jsonSerializer)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));

            BsonJsonConvention.Register(jsonSerializer);

            return true;
        }

        protected override string CollectionName()
        {
            var attribute = typeof(T).GetCustomAttributes(true).OfType<CollectionNameAttribute>().FirstOrDefault();

            var name = attribute?.Name ?? typeof(T).Name;

            return $"States_{name}";
        }

        public async Task<(T Value, bool Valid, long Version)> ReadAsync(DomainId key)
        {
            using (Telemetry.Activities.StartMethod<MongoSnapshotStore<T>>())
            {
                var existing =
                    await Collection.Find(x => x.DocumentId.Equals(key))
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (existing.Doc, true, existing.Version);
                }

                return (default!, true, EtagVersion.Empty);
            }
        }

        public async Task WriteAsync(DomainId key, T value, long oldVersion, long newVersion)
        {
            using (Telemetry.Activities.StartMethod<MongoSnapshotStore<T>>())
            {
                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u.Set(x => x.Doc, value));
            }
        }

        public Task WriteManyAsync(IEnumerable<(DomainId Key, T Value, long Version)> snapshots)
        {
            using (Telemetry.Activities.StartMethod<MongoSnapshotStore<T>>())
            {
                var writes = snapshots.Select(x => new ReplaceOneModel<MongoState<T>>(
                    Filter.Eq(y => y.DocumentId, x.Key),
                    new MongoState<T>
                    {
                        Doc = x.Value,
                        DocumentId = x.Key,
                        Version = x.Version
                    })
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

        public async Task ReadAllAsync(Func<T, long, Task> callback,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartMethod<MongoSnapshotStore<T>>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachAsync(x => callback(x.Doc, x.Version), ct);
            }
        }

        public async Task RemoveAsync(DomainId key)
        {
            using (Telemetry.Activities.StartMethod<MongoSnapshotStore<T>>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId.Equals(key));
            }
        }
    }
}
