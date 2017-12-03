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

        public async Task<(T Value, string Etag)> ReadAsync<T>(string key)
        {
            var collection = GetCollection<T>();

            var existing =
                await collection.Find(x => x.Id == key)
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (existing.Doc, existing.Etag);
            }

            return (default(T), null);
        }

        public async Task WriteAsync<T>(string key, T value, string oldEtag, string newEtag)
        {
            var collection = GetCollection<T>();

            try
            {
                await collection.UpdateOneAsync(
                    Builders<MongoState<T>>.Filter.And(
                        Builders<MongoState<T>>.Filter.Eq(x => x.Id, key),
                        Builders<MongoState<T>>.Filter.Eq(x => x.Etag, oldEtag)
                    ),
                    Builders<MongoState<T>>.Update
                        .Set(x => x.Doc, value)
                        .Set(x => x.Etag, newEtag),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingEtag =
                        await collection.Find(x => x.Id == key)
                            .Project<MongoState<T>>(Builders<MongoState<T>>.Projection.Exclude(x => x.Id)).FirstOrDefaultAsync();

                    if (existingEtag != null)
                    {
                        throw new InconsistentStateException(existingEtag.Etag, oldEtag, ex);
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
