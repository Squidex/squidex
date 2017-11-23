// ==========================================================================
//  MongoStateStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public sealed class MongoStateStore : IStateStore, IExternalSystem
    {
        private const string FieldId = "_id";
        private const string FieldDoc = "_doc";
        private const string FieldEtag = "_etag";
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        private static readonly FilterDefinitionBuilder<BsonDocument> Filter = Builders<BsonDocument>.Filter;
        private static readonly UpdateDefinitionBuilder<BsonDocument> Update = Builders<BsonDocument>.Update;
        private static readonly ProjectionDefinitionBuilder<BsonDocument> Projection = Builders<BsonDocument>.Projection;
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
                await collection.Find(Filter.Eq(FieldId, key))
                    .FirstOrDefaultAsync();

            if (existing != null)
            {
                var value = existing[FieldDoc].AsBsonDocument.ToJson().ToObject<T>(serializer);

                return (value, existing[FieldEtag].AsString);
            }

            return (default(T), null);
        }

        public async Task WriteAsync<T>(string key, T value, string oldEtag, string newEtag)
        {
            var collection = GetCollection<T>();

            var newData = JToken.FromObject(value, serializer).ToBson();

            try
            {
                await collection.UpdateOneAsync(
                    Filter.And(
                        Filter.Eq(FieldId, key),
                        Filter.Eq(FieldEtag, oldEtag)
                    ),
                    Update
                        .Set(FieldEtag, newEtag)
                        .Set(FieldDoc, newData),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingEtag =
                        await collection.Find(Filter.Eq(FieldId, key))
                            .Project<BsonDocument>(Projection.Exclude(FieldDoc)).FirstOrDefaultAsync();

                    if (existingEtag != null && existingEtag.Contains(FieldEtag))
                    {
                        throw new InconsistentStateException(existingEtag[FieldEtag].AsString, oldEtag, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private IMongoCollection<BsonDocument> GetCollection<T>()
        {
            return database.GetCollection<BsonDocument>($"States_{typeof(T).Name}");
        }
    }
}
