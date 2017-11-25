// ==========================================================================
//  MongoStateStore.cs
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
    public sealed class MongoStateStore : IStateStore, IExternalSystem
    {
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        private readonly IMongoDatabase database;
        private readonly JsonSerializer serializer;

        public MongoStateStore(IMongoDatabase database, JsonSerializer serializer)
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
                await collection.InsertOneAsync(
                    /*Builders<MongoState<T>>.Filter.And(
                        Builders<MongoState<T>>.Filter.Eq(nameof(MongoState<T>.Id), key),
                        Builders<MongoState<T>>.Filter.Eq(nameof(MongoState<T>.Etag), oldEtag)
                    ),*/
                    new MongoState<T> { Id = key, Etag = newEtag, Doc = value });
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
