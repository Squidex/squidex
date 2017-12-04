// ==========================================================================
//  EntityMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities
{
    public static class EntityMapper
    {
        public static T Update<T>(this T entity, SquidexCommand command, Action<T> updater = null) where T : IEntity
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();

            SetAppId(entity, command);
            SetVersion(entity);
            SetCreated(entity, timestamp);
            SetCreatedBy(entity, command);
            SetLastModified(entity, timestamp);
            SetLastModifiedBy(entity, command);

            updater?.Invoke(entity);

            return entity;
        }

        private static void SetLastModified(IEntity entity, Instant timestamp)
        {
            entity.LastModified = timestamp;
        }

        private static void SetCreated(IEntity entity, Instant timestamp)
        {
            if (entity.Created == default(Instant))
            {
                entity.Created = timestamp;
            }
        }

        private static void SetVersion(IEntity entity)
        {
            if (entity is IUpdateableEntityWithVersion withVersion)
            {
                withVersion.Version++;
            }
        }

        private static void SetCreatedBy(IEntity entity, SquidexCommand command)
        {
            if (entity is IUpdateableEntityWithCreatedBy withCreatedBy && withCreatedBy.CreatedBy == null)
            {
                withCreatedBy.CreatedBy = command.Actor;
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
