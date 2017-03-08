// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Events;
using Squidex.Infrastructure.CQRS.Events;
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
            if (entity is IEntityWithVersion withVersion)
            {
                withVersion.Version = headers.EventStreamNumber();
            }
        }

        private static void SetCreatedBy(SquidexEvent @event, MongoEntity entity)
        {
            if (entity is IEntityWithCreatedBy withCreatedBy)
            {
                withCreatedBy.CreatedBy = @event.Actor;
            }
        }

        private static void SetLastModifiedBy(SquidexEvent @event, MongoEntity entity)
        {
            if (entity is IEntityWithLastModifiedBy withModifiedBy)
            {
                withModifiedBy.LastModifiedBy = @event.Actor;
            }
        }

        private static void SetAppId(SquidexEvent @event, MongoEntity entity)
        {
            if (entity is IAppRefEntity appEntity && @event is AppEvent appEvent)
            {
                appEntity.AppId = appEvent.AppId.Id;
            }
        }
    }
}
