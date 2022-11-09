// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.MongoDb;

public abstract class MongoBase<TEntity>
{
    protected static readonly FilterDefinitionBuilder<TEntity> Filter =
        Builders<TEntity>.Filter;

    protected static readonly IndexKeysDefinitionBuilder<TEntity> Index =
        Builders<TEntity>.IndexKeys;

    protected static readonly ProjectionDefinitionBuilder<TEntity> Projection =
        Builders<TEntity>.Projection;

    protected static readonly SortDefinitionBuilder<TEntity> Sort =
        Builders<TEntity>.Sort;

    protected static readonly UpdateDefinitionBuilder<TEntity> Update =
        Builders<TEntity>.Update;

    protected static readonly BulkWriteOptions BulkUnordered =
        new BulkWriteOptions { IsOrdered = true };

    protected static readonly InsertManyOptions InsertUnordered =
        new InsertManyOptions { IsOrdered = true };

    protected static readonly ReplaceOptions UpsertReplace =
        new ReplaceOptions { IsUpsert = true };

    protected static readonly UpdateOptions Upsert =
        new UpdateOptions { IsUpsert = true };

    protected static readonly BsonDocument FindAll =
        new BsonDocument();

    static MongoBase()
    {
        BsonDefaultConventions.Register();
        BsonDomainIdSerializer.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonInstantSerializer.Register();
        BsonJsonConvention.Register();
        BsonJsonValueSerializer.Register();
        BsonStringSerializer<RefToken>.Register();
    }
}
