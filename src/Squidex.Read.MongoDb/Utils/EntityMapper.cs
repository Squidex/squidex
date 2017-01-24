// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read;
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable SuspiciousTypeConversion.Global

namespace Squidex.Read.MongoDb.Utils
{
    public static class EntityMapper
    {
        public static T Create<T>(EnvelopeHeaders headers, bool useAggregateId = true) where T : MongoEntity, new()
        {
            var entity = new T();

            UpdateWithId(headers, entity, useAggregateId);
            UpdateWithAppId(headers, entity);
            UpdateWithCreated(headers, entity);
            UpdateWithCreatedBy(headers, entity);

            return Update(entity, headers);
        }

        public static T Update<T>(T entity, EnvelopeHeaders headers) where T : MongoEntity
        {
            UpdateWithLastModified(headers, entity);
            UpdateWithLastModifiedBy(headers, entity);

            return entity;
        }

        private static void UpdateWithCreated(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.Created = headers.Timestamp().ToDateTimeUtc();
        }

        private static void UpdateWithLastModified(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.LastModified = headers.Timestamp().ToDateTimeUtc();
        }

        private static void UpdateWithCreatedBy(EnvelopeHeaders headers, MongoEntity entity)
        {
            var createdBy = entity as ITrackCreatedByEntity;

            if (createdBy != null)
            {
                createdBy.CreatedBy = headers.Actor();
            }
        }

        private static void UpdateWithLastModifiedBy(EnvelopeHeaders headers, MongoEntity entity)
        {
            var modifiedBy = entity as ITrackLastModifiedByEntity;

            if (modifiedBy != null)
            {
                modifiedBy.LastModifiedBy = headers.Actor();
            }
        }

        private static void UpdateWithAppId(EnvelopeHeaders headers, MongoEntity entity)
        {
            var appEntity = entity as IAppRefEntity;

            if (appEntity != null)
            {
                appEntity.AppId = headers.AppId();
            }
        }

        private static void UpdateWithId(EnvelopeHeaders headers, MongoEntity entity, bool useAggregateId)
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

        public static Task CreateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Action<T> updater, bool useAggregateId = true) where T : MongoEntity, new()
        {
            var entity = Create<T>(headers, useAggregateId);

            updater(entity);

            return collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task CreateAsync<T>(this IMongoCollection<T> collection, EnvelopeHeaders headers, Func<T, Task> updater, bool useAggregateId = true) where T : MongoEntity, new()
        {
            var entity = Create<T>(headers, useAggregateId);

            await updater(entity);

            await collection.InsertOneIfNotExistsAsync(entity);
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
