// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.MongoDb;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable SuspiciousTypeConversion.Global

namespace Squidex.Read.MongoDb.Utils
{
    public static class EntityMapper
    {
        public static T Create<T>(SquidexEvent @event, EnvelopeHeaders headers) where T : MongoEntity, new()
        {
            var entity = new T();

            SetId(headers, entity);

            SetVersion(headers, entity);
            SetCreated(headers, entity);
            SetCreatedBy(@event, entity);

            SetAppId(@event, entity);

            return Update(@event, headers, entity);
        }

        public static T Update<T>(SquidexEvent @event, EnvelopeHeaders headers, T entity) where T : MongoEntity, new()
        {
            SetVersion(headers, entity);
            SetLastModified(headers, entity);
            SetLastModifiedBy(@event, entity);

            return entity;
        }

        private static void SetId(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.Id = headers.AggregateId();
        }

        private static void SetCreated(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.Created = headers.Timestamp();
        }

        private static void SetLastModified(EnvelopeHeaders headers, MongoEntity entity)
        {
            entity.LastModified = headers.Timestamp();
        }

        private static void SetVersion(EnvelopeHeaders headers, MongoEntity entity)
        {
            var withVersion = entity as IEntityWithVersion;

            if (withVersion != null)
            {
                withVersion.Version = headers.EventNumber();
            }
        }

        private static void SetCreatedBy(SquidexEvent @event, MongoEntity entity)
        {
            var withCreatedBy = entity as IEntityWithCreatedBy;

            if (withCreatedBy != null)
            {
                withCreatedBy.CreatedBy = @event.Actor;
            }
        }

        private static void SetLastModifiedBy(SquidexEvent @event, MongoEntity entity)
        {
            var withModifiedBy = entity as IEntityWithLastModifiedBy;

            if (withModifiedBy != null)
            {
                withModifiedBy.LastModifiedBy = @event.Actor;
            }
        }

        private static void SetAppId(SquidexEvent @event, MongoEntity entity)
        {
            var appEntity = entity as IAppRefEntity;
            var appEvent = @event as AppEvent;

            if (appEntity != null && appEvent != null)
            {
                appEntity.AppId = appEvent.AppId.Id;
            }
        }
    }
}
