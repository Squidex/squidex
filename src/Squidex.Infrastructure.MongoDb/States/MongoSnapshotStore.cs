// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public class MongoSnapshotStore<T, TKey> : MongoRepositoryBase<MongoState<T, TKey>>, ISnapshotStore<T, TKey>, IInitializable
    {
        private readonly JsonSerializer serializer;

        public MongoSnapshotStore(IMongoDatabase database, JsonSerializer serializer)
            : base(database)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
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
                    await Collection.Find(x => x.Id.Equals(key))
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (existing.Doc, existing.Version);
                }

                return (default(T), EtagVersion.NotFound);
            }
        }

        public async Task WriteAsync(TKey key, T value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u.Set(x => x.Doc, value));
            }
        }

        public async Task ReadAllAsync(System.Func<T, long, Task> callback)
        {
            using (Profiler.TraceMethod<MongoSnapshotStore<T, TKey>>())
            {
                await Collection.Find(new BsonDocument()).ForEachAsync(x => callback(x.Doc, x.Version));
            }
        }
    }
}
