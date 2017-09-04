// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable SuspiciousTypeConversion.Global

namespace Squidex.Domain.Apps.Read.MongoDb.Utils
{
    public static class EntityMapper
    {
        public static T Create<T>(SquidexEvent @event, EnvelopeHeaders headers) where T : IMongoEntity, new()
        {
            var entity = new T();

            SetId(headers, entity);

            SetVersion(headers, entity);
            SetCreated(headers, entity);
            SetCreatedBy(@event, entity);

            SetAppId(@event, entity);

            return Update(@event, headers, entity);
        }

        public static T Update<T>(SquidexEvent @event, EnvelopeHeaders headers, T entity) where T : IMongoEntity, new()
        {
            SetVersion(headers, entity);
            SetLastModified(headers, entity);
            SetLastModifiedBy(@event, entity);

            return entity;
        }

        private static void SetId(EnvelopeHeaders headers, IMongoEntity entity)
        {
            entity.Id = headers.AggregateId();
        }

        private static void SetCreated(EnvelopeHeaders headers, IMongoEntity entity)
        {
            entity.Created = headers.Timestamp();
        }

        private static void SetLastModified(EnvelopeHeaders headers, IMongoEntity entity)
        {
            entity.LastModified = headers.Timestamp();
        }

        private static void SetVersion(EnvelopeHeaders headers, IMongoEntity entity)
        {
            if (entity is IEntityWithVersion withVersion)
            {
                withVersion.Version = headers.EventStreamNumber();
            }
        }

        private static void SetCreatedBy(SquidexEvent @event, IMongoEntity entity)
        {
            if (entity is IEntityWithCreatedBy withCreatedBy)
            {
                withCreatedBy.CreatedBy = @event.Actor;
            }
        }

        private static void SetLastModifiedBy(SquidexEvent @event, IMongoEntity entity)
        {
            if (entity is IEntityWithLastModifiedBy withModifiedBy)
            {
                withModifiedBy.LastModifiedBy = @event.Actor;
            }
        }

        private static void SetAppId(SquidexEvent @event, IMongoEntity entity)
        {
            if (entity is IAppRefEntity app && @event is AppEvent appEvent)
            {
                app.AppId = appEvent.AppId.Id;
            }
        }
    }
}
