// ==========================================================================
//  EntityMapper.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.MongoDb;

namespace PinkParrot.Read.Repositories.Implementations
{
    public static class EntityMapper
    {
        public static T Create<T>(EnvelopeHeaders headers) where T : IEntity, new()
        {
            var timestamp = headers.Timestamp().ToDateTimeUtc();

            var entity = new T { Id = headers.AggregateId(), Created = timestamp };

            var tenantEntity = entity as ITenantEntity;

            if (tenantEntity != null)
            {
                tenantEntity.TenantId = headers.TenantId();
            }
            
            return Update(entity, headers);
        }

        public static BsonDocument ToJsonBsonDocument<T>(this T value, JsonSerializerSettings settings)
        {
            var json = JsonConvert.SerializeObject(value, settings).Replace("$type", "§type");

            return BsonDocument.Parse(json);
        }

        public static T ToJsonObject<T>(this BsonDocument document, JsonSerializerSettings settings)
        {
            var json = document.ToJson().Replace("§type", "$type");

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static T Update<T>(T entity, EnvelopeHeaders headers) where T : IEntity
        {
            var timestamp = headers.Timestamp().ToDateTimeUtc();

            entity.LastModified = timestamp;

            return entity;
        }

        public static Task CreateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater) where T : class, IEntity, new()
        {
            var entity = Create<T>(headers);

            updater(entity);

            return collection.InsertOneIfExistsAsync(entity);
        }

        public static async Task UpdateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater) where T : class, IEntity
        {
            var entity = await collection.Find(t => t.Id == headers.AggregateId()).FirstOrDefaultAsync();

            if (entity == null)
            {
                return;
            }

            Update(entity, headers);

            updater(entity);

            var result = await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }
    }
}
