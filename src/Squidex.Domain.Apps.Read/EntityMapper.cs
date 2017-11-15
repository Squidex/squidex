// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read
{
    public static class EntityMapper
    {
        public static T Create<T>(SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater = null) where T : IEntity, new()
        {
            var entity = new T();

            SetId(headers, entity);

            SetVersion(headers, entity);
            SetCreated(headers, entity);
            SetCreatedBy(@event, entity);

            SetAppId(@event, entity);

            return entity.Update(@event, headers, updater);
        }

        public static T Update<T>(this T entity, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater = null) where T : IEntity, new()
        {
            SetVersion(headers, entity);
            SetLastModified(headers, entity);
            SetLastModifiedBy(@event, entity);

            updater?.Invoke(entity);

            return entity;
        }

        private static void SetId(EnvelopeHeaders headers, IEntity entity)
        {
            entity.Id = headers.AggregateId();
        }

        private static void SetCreated(EnvelopeHeaders headers, IEntity entity)
        {
            entity.Created = headers.Timestamp();
        }

        private static void SetLastModified(EnvelopeHeaders headers, IEntity entity)
        {
            entity.LastModified = headers.Timestamp();
        }

        private static void SetVersion(EnvelopeHeaders headers, IEntity entity)
        {
            if (entity is IUpdateableEntityWithVersion withVersion)
            {
                withVersion.Version = headers.EventStreamNumber();
            }
        }

        private static void SetCreatedBy(SquidexEvent @event, IEntity entity)
        {
            if (entity is IUpdateableEntityWithCreatedBy withCreatedBy)
            {
                withCreatedBy.CreatedBy = @event.Actor;
            }
        }

        private static void SetLastModifiedBy(SquidexEvent @event, IEntity entity)
        {
            if (entity is IUpdateableEntityWithLastModifiedBy withModifiedBy)
            {
                withModifiedBy.LastModifiedBy = @event.Actor;
            }
        }

        private static void SetAppId(SquidexEvent @event, IEntity entity)
        {
            if (entity is IUpdateableEntityWithAppRef app && @event is AppEvent appEvent)
            {
                app.AppId = appEvent.AppId.Id;
            }
        }
    }
}
