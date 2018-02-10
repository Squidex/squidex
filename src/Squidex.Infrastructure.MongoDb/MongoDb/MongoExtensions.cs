// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.States;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Infrastructure.MongoDb
{
    public static class MongoExtensions
    {
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };

        public static async Task<bool> InsertOneIfNotExistsAsync<T>(this IMongoCollection<T> collection, T document)
        {
            try
            {
                await collection.InsertOneAsync(document);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public static async Task TryDropOneAsync<T>(this IMongoIndexManager<T> indexes, string name)
        {
            try
            {
                await indexes.DropOneAsync(name);
            }
            catch
            {
                /* NOOP */
            }
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include1,
            Expression<Func<TDocument, object>> include2)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include1,
            Expression<Func<TDocument, object>> include2,
            Expression<Func<TDocument, object>> include3)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2).Include(include3));
        }

        public static async Task UpsertVersionedAsync<T, TKey>(this IMongoCollection<T> collection, TKey key, long oldVersion, long newVersion, Func<UpdateDefinition<T>, UpdateDefinition<T>> updater) where T : IVersionedEntity<TKey>
        {
            try
            {
                var update = updater(Builders<T>.Update.Set(x => x.Version, newVersion));

                await collection.UpdateOneAsync(x => x.Id.Equals(key) && x.Version == oldVersion,
                    update
                        .Set(x => x.Version, newVersion),
                    Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await collection.Find(x => x.Id.Equals(key)).Only(x => x.Id, x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion[nameof(IVersionedEntity<TKey>.Version)].AsInt64, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task UpsertVersionedAsync<T, TKey>(this IMongoCollection<T> collection, TKey key, long oldVersion, long newVersion, T doc) where T : IVersionedEntity<TKey>
        {
            try
            {
                await collection.ReplaceOneAsync(x => x.Id.Equals(key) && x.Version == oldVersion, doc, Upsert);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await collection.Find(x => x.Id.Equals(key)).Only(x => x.Id, x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion[nameof(IVersionedEntity<TKey>.Version)].AsInt64, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
