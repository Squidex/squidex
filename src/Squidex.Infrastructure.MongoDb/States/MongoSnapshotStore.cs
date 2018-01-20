// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
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
            return $"States_{typeof(T).Name}";
        }

        public async Task<(T Value, long Version)> ReadAsync(TKey key)
        {
            var existing =
                await Collection.Find(x => Equals(x.Id, key))
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.Doc, existing.Version);
            }

            return (default(T), EtagVersion.NotFound);
        }

        public Task WriteAsync(TKey key, T value, long oldVersion, long newVersion)
        {
            return Collection.UpsertVersionedAsync(key, oldVersion, newVersion, u => u.Set(x => x.Doc, value));
        }
    }
}
