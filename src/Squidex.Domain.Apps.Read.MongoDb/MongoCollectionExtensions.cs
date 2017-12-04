﻿// ==========================================================================
//  MongoCollectionExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb
{
    public static class MongoCollectionExtensions
    {
        public static Task CreateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IEntity, new()
        {
            var entity = EntityMapper.Create(@event, headers, updater);

            return collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task UpdateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IEntity, new()
        {
            var entity =
                await collection.Find(t => t.Id == headers.AggregateId())
                    .FirstOrDefaultAsync();

            if (entity == null)
            {
                throw new DomainObjectNotFoundException(headers.AggregateId().ToString(), typeof(T));
            }

            await collection.UpdateAsync(@event, headers, entity, updater);
        }

        public static async Task<bool> TryUpdateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IEntity, new()
        {
            var entity =
                await collection.Find(t => t.Id == headers.AggregateId())
                    .FirstOrDefaultAsync();

            if (entity != null)
            {
                if (entity is IEntityWithVersion withVersion)
                {
                    var eventVersion = headers.EventStreamNumber();

                    if (eventVersion <= withVersion.Version)
                    {
                        return false;
                    }
                }

                await collection.UpdateAsync(@event, headers, entity, updater);

                return true;
            }

            return false;
        }

        private static async Task UpdateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, T entity, Action<T> updater) where T : class, IEntity, new()
        {
            entity.Update(@event, headers, updater);

            await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }
    }
}
