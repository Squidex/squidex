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
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable SuspiciousTypeConversion.Global

namespace Squidex.Store.MongoDb.Utils
{
    public static class EntityMapper
    {
        public static T Create<T>(EnvelopeHeaders headers, bool useAggregateId = true) where T : MongoEntity, new()
        {
            var entity = new T();

            AssignId(headers, entity, useAggregateId);
            AssignAppId(headers, entity);
            AssignCreated(headers, entity);
            AssignCreatedBy(headers, entity);

            return Update(entity, headers);
        }

        public static T Update<T>(T entity, EnvelopeHeaders headers) where T : MongoEntity
        {
            AssignLastModified(headers, entity);
            AssignLastModifiedBy(headers, entity);

            return entity;
        }

        private static void AssignCreated(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.Created = headers.Timestamp().ToDateTimeUtc();
        }

        private static void AssignLastModified(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.LastModified = headers.Timestamp().ToDateTimeUtc();
        }

        private static void AssignCreatedBy(EnvelopeHeaders headers, MongoEntity entity)
        {
            var createdBy = entity as ITrackCreatedByEntity;

            if (createdBy != null)
            {
                createdBy.CreatedBy = headers.Actor();
            }
        }

        private static void AssignLastModifiedBy(EnvelopeHeaders headers, MongoEntity entity)
        {
            var modifiedBy = entity as ITrackLastModifiedByEntity;

            if (modifiedBy != null)
            {
                modifiedBy.LastModifiedBy = headers.Actor();
            }
        }

        private static void AssignAppId(EnvelopeHeaders headers, MongoEntity entity)
        {
            var appEntity = entity as IAppRefEntity;

            if (appEntity != null)
            {
                appEntity.AppId = headers.AppId();
            }
        }

        private static void AssignId(EnvelopeHeaders headers, MongoEntity entity, bool useAggregateId)
        {
            if (useAggregateId)
            {
                entity.Id = headers.AggregateId();
            }
            else
            {
                entity.Id = Guid.NewGuid();
            }
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

        public static Task CreateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater, bool useAggregateId = true) where T : MongoEntity, new()
        {
            var entity = Create<T>(headers, useAggregateId);

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
