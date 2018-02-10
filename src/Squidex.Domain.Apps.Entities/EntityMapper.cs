// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public static class EntityMapper
    {
        public static T Update<T>(this T entity, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater = null) where T : IEntity
        {
            SetId(entity, headers);
            SetCreated(entity, headers);
            SetCreatedBy(entity, @event);
            SetLastModified(entity, headers);
            SetLastModifiedBy(entity, @event);
            SetVersion(entity, headers);

            updater?.Invoke(entity);

            return entity;
        }

        private static void SetId(IEntity entity, EnvelopeHeaders headers)
        {
            if (entity is IUpdateableEntity updateable && updateable.Id == Guid.Empty)
            {
                updateable.Id = headers.AggregateId();
            }
        }

        private static void SetVersion(IEntity entity, EnvelopeHeaders headers)
        {
            if (entity is IUpdateableEntityWithVersion updateable)
            {
                updateable.Version = headers.EventStreamNumber();
            }
        }

        private static void SetCreated(IEntity entity, EnvelopeHeaders headers)
        {
            if (entity is IUpdateableEntity updateable && updateable.Created == default(Instant))
            {
                updateable.Created = headers.Timestamp();
            }
        }

        private static void SetCreatedBy(IEntity entity, SquidexEvent @event)
        {
            if (entity is IUpdateableEntityWithCreatedBy withCreatedBy && withCreatedBy.CreatedBy == null)
            {
                withCreatedBy.CreatedBy = @event.Actor;
            }
        }

        private static void SetLastModified(IEntity entity, EnvelopeHeaders headers)
        {
            if (entity is IUpdateableEntity updateable)
            {
                updateable.LastModified = headers.Timestamp();
            }
        }

        private static void SetLastModifiedBy(IEntity entity, SquidexEvent @event)
        {
            if (entity is IUpdateableEntityWithLastModifiedBy withModifiedBy)
            {
                withModifiedBy.LastModifiedBy = @event.Actor;
            }
        }
    }
}
