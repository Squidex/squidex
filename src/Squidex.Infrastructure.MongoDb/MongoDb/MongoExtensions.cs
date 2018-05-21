// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> exclude)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> exclude1,
            Expression<Func<TDocument, object>> exclude2)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> exclude1,
            Expression<Func<TDocument, object>> exclude2,
            Expression<Func<TDocument, object>> exclude3)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2).Exclude(exclude3));
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

        public static async Task ForEachPipelineAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cursor = await source.ToCursorAsync(cancellationToken);

            await cursor.ForEachPipelineAsync(processor, cancellationToken);
        }

        public static async Task ForEachPipelineAsync<TDocument>(this IAsyncCursor<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var selfToken = new CancellationTokenSource())
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(selfToken.Token, cancellationToken))
                {
                    var actionBlock =
                        new ActionBlock<TDocument>(async x =>
                            {
                                if (!combined.IsCancellationRequested)
                                {
                                    await processor(x);
                                }
                            },
                            new ExecutionDataflowBlockOptions
                            {
                                MaxDegreeOfParallelism = 1,
                                MaxMessagesPerTask = 1,
                                BoundedCapacity = 100
                            });
                    try
                    {
                        await source.ForEachAsync(async i =>
                        {
                            if (!await actionBlock.SendAsync(i, combined.Token))
                            {
                                selfToken.Cancel();
                            }
                        }, combined.Token);

                        actionBlock.Complete();
                    }
                    catch (Exception ex)
                    {
                        ((IDataflowBlock)actionBlock).Fault(ex);
                    }
                    finally
                    {
                        await actionBlock.Completion;
                    }
                }
            }
        }
    }
}
