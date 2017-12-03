// ==========================================================================
//  MongoSnapshotStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.States
{
    public sealed class MongoSnapshotStore : ISnapshotStore, IExternalSystem
    {
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        private readonly IMongoDatabase database;
        private readonly JsonSerializer serializer;

        public MongoSnapshotStore(IMongoDatabase database, JsonSerializer serializer)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNull(serializer, nameof(serializer));

            this.database = database;
            this.serializer = serializer;
        }

        public void Connect()
        {
            try
            {
                database.ListCollections();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"MongoDb connection failed to connect to database {database.DatabaseNamespace.DatabaseName}", ex);
            }
        }

        public async Task<(T Value, long Version)> ReadAsync<T>(string key)
        {
            var collection = GetCollection<T>();

            var existing =
                await collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.Doc, existing.Version);
            }

            return (default(T), -1);
        }

        public async Task WriteAsync<T>(string key, T value, long oldVersion, long newVersion)
        {
            var collection = GetCollection<T>();

            try
            {
                await collection.UpdateOneAsync(
                    Builders<MongoState<T>>.Filter.And(
                        Builders<MongoState<T>>.Filter.Eq(x => x.Id, key),
                        Builders<MongoState<T>>.Filter.Eq(x => x.Version, oldVersion)
                    ),
                    Builders<MongoState<T>>.Update
                        .Set(x => x.Doc, value)
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await collection.Find(x => x.Id == key)
                            .Project<MongoState<T>>(Builders<MongoState<T>>.Projection.Exclude(x => x.Id)).FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion.Version, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private IMongoCollection<MongoState<T>> GetCollection<T>()
        {
            return database.GetCollection<MongoState<T>>($"States_{typeof(T).Name}");
        }
    }
}
