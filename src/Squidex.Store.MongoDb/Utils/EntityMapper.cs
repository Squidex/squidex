// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Read;

namespace Squidex.Store.MongoDb.Utils
{
    public static class EntityMapper
    {
        public static T Create<T>(EnvelopeHeaders headers) where T : MongoEntity, new()
        {
            var timestamp = headers.Timestamp().ToDateTimeUtc();

            var entity = new T { Id = headers.AggregateId(), Created = timestamp };

            var appEntity = entity as IAppEntity;

            if (appEntity != null)
            {
                appEntity.AppId = headers.AppId();
            }
            
            return Update(entity, headers);
        }

        public static BsonDocument ToBsonDocument(this JToken value)
        {
            var json = value.ToString().Replace("$type", "§type");

            return BsonDocument.Parse(json);
        }

        public static JToken ToJToken(this BsonDocument document)
        {
            var json = document.ToJson().Replace("§type", "$type");

            return JToken.Parse(json);
        }

        public static T Update<T>(T entity, EnvelopeHeaders headers) where T : MongoEntity
        {
            var timestamp = headers.Timestamp().ToDateTimeUtc();

            entity.LastModified = timestamp;

            return entity;
        }

        public static Task CreateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater) where T : MongoEntity, new()
        {
            var entity = Create<T>(headers);

            updater(entity);

            return collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task UpdateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater) where T : MongoEntity
        {
            var entity = await collection.Find(t => t.Id == headers.AggregateId()).FirstOrDefaultAsync();

            if (entity == null)
            {
                return;
            }

            Update(entity, headers);
            updater(entity);

            await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }
    }
}
