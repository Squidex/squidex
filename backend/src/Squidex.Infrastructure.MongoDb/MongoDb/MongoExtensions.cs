// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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

        public static Task<bool> AnyAsync<T>(this IMongoCollection<T> collection)
        {
            var find = collection.Find(new BsonDocument()).Limit(1);

            return find.AnyAsync();
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

        public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
            Expression<Func<T, object>> include)
        {
            return find.Project<BsonDocument>(Builders<T>.Projection.Include(include));
        }

        public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
            Expression<Func<T, object>> include1,
            Expression<Func<T, object>> include2)
        {
            return find.Project<BsonDocument>(Builders<T>.Projection.Include(include1).Include(include2));
        }

        public static IFindFluent<T, BsonDocument> Only<T>(this IFindFluent<T, T> find,
            Expression<Func<T, object>> include1,
            Expression<Func<T, object>> include2,
            Expression<Func<T, object>> include3)
        {
            return find.Project<BsonDocument>(Builders<T>.Projection.Include(include1).Include(include2).Include(include3));
        }

        public static IFindFluent<T, T> Not<T>(this IFindFluent<T, T> find,
            Expression<Func<T, object>> exclude)
        {
            return find.Project<T>(Builders<T>.Projection.Exclude(exclude));
        }

        public static IFindFluent<T, T> Not<T>(this IFindFluent<T, T> find,
            Expression<Func<T, object>> exclude1,
            Expression<Func<T, object>> exclude2)
        {
            return find.Project<T>(Builders<T>.Projection.Exclude(exclude1).Exclude(exclude2));
        }

        public static async Task UpsertVersionedAsync<T, TKey>(this IMongoCollection<T> collection, TKey key, long oldVersion, long newVersion, Func<UpdateDefinition<T>, UpdateDefinition<T>> updater)
            where T : IVersionedEntity<TKey> where TKey : notnull
        {
            try
            {
                var update = updater(Builders<T>.Update.Set(x => x.Version, newVersion));

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
                    var field = Field.Of<T>(x => nameof(x.Version));

                    throw new InconsistentStateException(existingVersion[field].AsInt64, oldVersion, ex);
                }
                else
                {
                    throw new InconsistentStateException(EtagVersion.Any, oldVersion, ex);
                }
            }
        }

        public static async Task UpsertVersionedAsync<T, TKey>(this IMongoCollection<T> collection, TKey key, long oldVersion, long newVersion, T document)
            where T : IVersionedEntity<TKey> where TKey : notnull
        {
            try
            {
                document.DocumentId = key;
                document.Version = newVersion;

                if (oldVersion > EtagVersion.Any)
                {
                    await collection.ReplaceOneAsync(x => x.DocumentId.Equals(key) && x.Version == oldVersion, document, UpsertReplace);
                }
                else
                {
                    await collection.ReplaceOneAsync(x => x.DocumentId.Equals(key), document, UpsertReplace);
                }
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                var existingVersion =
                    await collection.Find(x => x.DocumentId.Equals(key)).Only(x => x.DocumentId, x => x.Version)
                        .FirstOrDefaultAsync();

                if (existingVersion != null)
                {
                    var field = Field.Of<T>(x => nameof(x.Version));

                    throw new InconsistentStateException(existingVersion[field].AsInt64, oldVersion, ex);
                }
                else
                {
                    throw new InconsistentStateException(EtagVersion.Any, oldVersion, ex);
                }
            }
        }

        public static async Task ForEachPipedAsync<T>(this IAsyncCursorSource<T> source, Func<T, Task> processor, CancellationToken cancellationToken = default)
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken))
            {
                await cursor.ForEachPipedAsync(processor, cancellationToken);
            }
        }

        public static async Task ForEachPipedAsync<T>(this IAsyncCursor<T> source, Func<T, Task> processor, CancellationToken cancellationToken = default)
        {
            using (var selfToken = new CancellationTokenSource())
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(selfToken.Token, cancellationToken))
                {
                    var actionBlock =
                        new ActionBlock<T>(async x =>
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

        public static async Task<Version> GetVersionAsync(this IMongoDatabase database)
        {
            var command =
                new BsonDocumentCommand<BsonDocument>(new BsonDocument
                {
                    { "buildInfo", 1 }
                });

            var result = await database.RunCommandAsync(command);

            return Version.Parse(result["version"].AsString);
        }
    }
}
