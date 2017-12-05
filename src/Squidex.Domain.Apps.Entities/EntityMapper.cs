// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public static class EntityMapper
    {
        public static T Update<T>(this T entity, SquidexCommand command, Action<T> updater = null) where T : IEntity
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();

            SetId(entity, command);
            SetAppId(entity, command);
            SetCreated(entity, timestamp);
            SetCreatedBy(entity, command);
            SetLastModified(entity, timestamp);
            SetLastModifiedBy(entity, command);
            SetVersion(entity);

            updater?.Invoke(entity);

            return entity;
        }

        private static void SetId(IEntity entity, SquidexCommand command)
        {
            if (entity is IUpdateableEntity updateable && command is IAggregateCommand aggregateCommand)
            {
                updateable.Id = aggregateCommand.AggregateId;
            }
        }

        private static void SetVersion(IEntity entity)
        {
            if (entity is IUpdateableEntityWithVersion withVersion)
            {
                withVersion.Version++;
            }
        }

        private static void SetCreated(IEntity entity, Instant timestamp)
        {
            if (entity is IUpdateableEntity updateable && updateable.Created == default(Instant))
            {
                updateable.Created = timestamp;
            }
        }

        private static void SetCreatedBy(IEntity entity, SquidexCommand command)
        {
            if (entity is IUpdateableEntityWithCreatedBy withCreatedBy && withCreatedBy.CreatedBy == null)
            {
                withCreatedBy.CreatedBy = command.Actor;
            }
        }

        private static void SetLastModified(IEntity entity, Instant timestamp)
        {
            if (entity is IUpdateableEntity updateable)
            {
                updateable.LastModified = timestamp;
            }
        }

        private static void SetLastModifiedBy(IEntity entity, SquidexCommand command)
        {
            if (entity is IUpdateableEntityWithLastModifiedBy withModifiedBy)
            {
                withModifiedBy.LastModifiedBy = command.Actor;
            }
        }

        private static void SetAppId(IEntity entity, SquidexCommand command)
        {
            if (entity is IUpdateableEntityWithAppRef appEntity && command is AppCommand appCommand)
            {
                appEntity.AppId = appCommand.AppId.Id;
            }
        }
    }
}
