// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Log;

namespace Squidex.Infrastructure.States
{
    public class MongoSnapshotStore<T, TKey> : MongoRepositoryBase<MongoState<T, TKey>>, ISnapshotStore<T, TKey> where TKey : notnull
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

        public async Task<(T Value, long Version)> ReadAsync(TKey key)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                var existing =
                    await Collection.Find(x => x.DocumentId.Equals(key))
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (existing.Doc, existing.Version);
                }

                return (default!, EtagVersion.NotFound);
            }
        }

        public async Task WriteAsync(TKey key, T value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u.Set(x => x.Doc, value));
            }
        }

        public async Task ReadAllAsync(Func<T, long, Task> callback, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipedAsync(x => callback(x.Doc, x.Version), ct);
            }
        }

        public async Task RemoveAsync(TKey key)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId.Equals(key));
            }
        }
    }
}
