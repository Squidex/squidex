// ==========================================================================
//  CollectionExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Read.MongoDb.Utils
{
    public static class CollectionExtensions
    {
        public static Task CreateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : MongoEntity, new()
        {
            var entity = EntityMapper.Create<T>(@event, headers);

            updater(entity);

            return collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task CreateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Func<T, Task> updater) where T : MongoEntity, new()
        {
            var entity = EntityMapper.Create<T>(@event, headers);

            await updater(entity);

            await collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task UpdateAsync<T>(this IMongoCollection<T> collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : MongoEntity, new()
        {
            var entity = await collection.Find(t => t.Id == headers.AggregateId()).FirstOrDefaultAsync();

            if (entity == null)
            {
                throw new DomainObjectNotFoundException(headers.AggregateId().ToString(), typeof(T));
            }

            EntityMapper.Update(@event, headers, entity);

            updater(entity);

            await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }
    }
}
