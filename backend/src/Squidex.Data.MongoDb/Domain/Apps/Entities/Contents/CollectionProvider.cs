// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using MongoDB.Driver;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

internal class CollectionProvider(IMongoClient mongoClient, string prefixDatabase, string prefixCollection)
{
    private readonly ConcurrentDictionary<(DomainId AppId, DomainId SchemaId), Lazy<Task<IMongoCollection<MongoContentEntity>>>> collections =
        new ConcurrentDictionary<(DomainId AppId, DomainId SchemaId), Lazy<Task<IMongoCollection<MongoContentEntity>>>>();

    public async Task<IMongoCollection<MongoContentEntity>> GetCollectionAsync(DomainId appId, DomainId schemaId)
    {
        var key = (appId, schemaId);

        // The lazy ensures that the indexes are only created once, even when the same collection is
        // requested concurrently. GetOrAdd alone can run the factory several times for one key.
        var collection = collections.GetOrAdd(key, CreateLazyCollection);

        try
        {
            return await collection.Value;
        }
        catch
        {
            // A failed attempt must not stay in the cache. Creating the indexes can fail for a
            // transient reason and the collection would be unusable until the process is restarted.
            // Only remove our own entry, so that a newer successful one is not thrown away.
            collections.TryRemove(new KeyValuePair<(DomainId AppId, DomainId SchemaId), Lazy<Task<IMongoCollection<MongoContentEntity>>>>(key, collection));
            throw;
        }
    }

    private Lazy<Task<IMongoCollection<MongoContentEntity>>> CreateLazyCollection((DomainId AppId, DomainId SchemaId) key)
    {
        return new Lazy<Task<IMongoCollection<MongoContentEntity>>>(
            () => CreateCollectionAsync(key),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private async Task<IMongoCollection<MongoContentEntity>> CreateCollectionAsync((DomainId AppId, DomainId SchemaId) key)
    {
        var (appId, schemaId) = key;

        var schemaDatabase = mongoClient.GetDatabase($"{prefixDatabase}_{appId}");
        var schemaCollection = schemaDatabase.GetCollection<MongoContentEntity>($"{prefixCollection}_{schemaId}");

        await schemaCollection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<MongoContentEntity>(
                    Builders<MongoContentEntity>.IndexKeys
                        .Descending(x => x.LastModified)
                        .Ascending(x => x.Id)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.ReferencedIds)),
                new CreateIndexModel<MongoContentEntity>(
                    Builders<MongoContentEntity>.IndexKeys
                        .Ascending(x => x.IndexedSchemaId)
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.LastModified)),
            ]);

        return schemaCollection;
    }
}
