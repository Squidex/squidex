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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Infrastructure.States;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Infrastructure.MongoDb
{
    public static class MongoExtensions
    {
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        private static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };

        public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName)
        {
            var options = new ListCollectionNamesOptions
            {
                Filter = new BsonDocument("name", collectionName)
            };

            var collections = await database.ListCollectionNamesAsync(options);

            return await collections.AnyAsync();
        }

        public static async Task<bool> InsertOneIfNotExistsAsync<T>(this IMongoCollection<T> collection, T document, CancellationToken ct = default)
        {
            try
            {
                await collection.InsertOneAsync(document, null, ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
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

        public static async Task UpsertVersionedAsync<TEntity, TKey>(this IMongoCollection<TEntity> collection, TKey key, long oldVersion, long newVersion, Func<UpdateDefinition<TEntity>, UpdateDefinition<TEntity>> updater)
            where TEntity : IVersionedEntity<TKey>
            where TKey : notnull
        {
            try
            {
                var update = updater(Builders<TEntity>.Update.Set(x => x.Version, newVersion));

                if (oldVersion > EtagVersion.Any)
                {
                    await collection.UpdateOneAsync(x => x.DocumentId.Equals(key) && x.Version == oldVersion, update, Upsert);
                }
                else
                {
                    await collection.UpdateOneAsync(x => x.DocumentId.Equals(key), update, Upsert);
                }
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                var existingVersion =
                    await collection.Find(x => x.DocumentId.Equals(key)).Only(x => x.DocumentId, x => x.Version)
                        .FirstOrDefaultAsync();

                if (existingVersion != null)
                {
                    var versionField = GetVersionField<TEntity, TKey>();

                    throw new InconsistentStateException(existingVersion[versionField].AsInt64, oldVersion, ex);
                }
                else
                {
                    throw new InconsistentStateException(EtagVersion.Any, oldVersion, ex);
                }
            }
        }

        public static async Task UpsertVersionedAsync<TEntity, TKey>(this IMongoCollection<TEntity> collection, TKey key, long oldVersion, long newVersion, TEntity doc)
            where TEntity : IVersionedEntity<TKey>
            where TKey : notnull
        {
            try
            {
                doc.DocumentId = key;
                doc.Version = newVersion;

                if (oldVersion > EtagVersion.Any)
                {
                    await collection.ReplaceOneAsync(x => x.DocumentId.Equals(key) && x.Version == oldVersion, doc, UpsertReplace);
                }
                else
                {
                    await collection.ReplaceOneAsync(x => x.DocumentId.Equals(key), doc, UpsertReplace);
                }
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                var existingVersion =
                    await collection.Find(x => x.DocumentId.Equals(key)).Only(x => x.DocumentId, x => x.Version)
                        .FirstOrDefaultAsync();

                if (existingVersion != null)
                {
                    var versionField = GetVersionField<TEntity, TKey>();

                    throw new InconsistentStateException(existingVersion[versionField].AsInt64, oldVersion, ex);
                }
                else
                {
                    throw new InconsistentStateException(EtagVersion.Any, oldVersion, ex);
                }
            }
        }

        private static string GetVersionField<TEntity, TKey>()
            where TEntity : IVersionedEntity<TKey>
            where TKey : notnull
        {
            return BsonClassMap.LookupClassMap(typeof(TEntity)).GetMemberMap(nameof(IVersionedEntity<TKey>.Version)).ElementName;
        }

        public static async Task ForEachPipedAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default)
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken))
            {
                await cursor.ForEachPipedAsync(processor, cancellationToken);
            }
        }

        public static async Task ForEachPipedAsync<TDocument>(this IAsyncCursor<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default)
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
                                BoundedCapacity = Batching.BufferSize
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
