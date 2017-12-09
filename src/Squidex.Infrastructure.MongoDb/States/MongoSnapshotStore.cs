// ==========================================================================
//  MongoSnapshotStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public class MongoSnapshotStore<T> : MongoRepositoryBase<MongoState<T>>, ISnapshotStore<T>, IExternalSystem
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

        public async Task<(T Value, long Version)> ReadAsync(string key)
        {
            var existing =
                await Collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.Doc, existing.Version);
            }

            return (default(T), -1);
        }

        public async Task WriteAsync(string key, T value, long oldVersion, long newVersion)
        {
            try
            {
                await Collection.UpdateOneAsync(x => x.Id == key && x.Version == oldVersion,
                    Update
                        .Set(x => x.Doc, value)
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == key).Only(x => x.Id, x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion["Version"].AsInt64, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
