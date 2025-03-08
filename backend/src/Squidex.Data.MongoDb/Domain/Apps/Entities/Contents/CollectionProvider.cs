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
    private readonly ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>> collections =
        new ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>>();

    public Task<IMongoCollection<MongoContentEntity>> GetCollectionAsync(DomainId appId, DomainId schemaId)
    {
        return collections.GetOrAdd((appId, schemaId), CreateCollectionAsync);
    }

    private async Task<IMongoCollection<MongoContentEntity>> CreateCollectionAsync((DomainId, DomainId) key)
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
