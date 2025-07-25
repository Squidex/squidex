﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public abstract class MongoTextIndexBase<T>(IMongoDatabase database, string shardKey, CommandFactory<T> factory) : MongoRepositoryBase<MongoTextIndexEntity<T>>(database), ITextIndex, IDeleter where T : class
{
    protected sealed class MongoTextResult
    {
        [BsonId]
        [BsonElement]
        public ObjectId Id { get; set; }

        [BsonRequired]
        [BsonElement("c")]
        public DomainId ContentId { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("score")]
        public double Score { get; set; }
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<T>> collection,
        CancellationToken ct)
    {
        var isFerreDb = await Database.IsFerretDbAsync(ct);

        if (isFerreDb)
        {
            await collection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<MongoTextIndexEntity<T>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ContentId)),

                new CreateIndexModel<MongoTextIndexEntity<T>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.SchemaId)
                        .Ascending(x => x.GeoField)),

                new CreateIndexModel<MongoTextIndexEntity<T>>(
                    Index
                        .Geo2DSphere(x => x.GeoObject)),
            ], ct);
        }
        else
        {
            await collection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<MongoTextIndexEntity<T>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ContentId)),

                new CreateIndexModel<MongoTextIndexEntity<T>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.SchemaId)
                        .Ascending(x => x.GeoField)
                        .Geo2DSphere(x => x.GeoObject)),
            ], ct);
        }
    }

    protected override string CollectionName()
    {
        return $"TextIndex2{shardKey}";
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
    }

    Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.SchemaId, schema.Id), ct);
    }

    public async virtual Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        var writes = new List<WriteModel<MongoTextIndexEntity<T>>>(commands.Length);

        foreach (var command in commands)
        {
            factory.CreateCommands(command, writes);
        }

        if (writes.Count == 0)
        {
            return;
        }

        try
        {
            await Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }
        catch (MongoBulkWriteException ex)
        {
            // Ignore invalid geo data when writing content. Our insert is unordered anyway.
            if (ex.WriteErrors.Any(error => error.Code != MongoDbErrorCodes.Errror16755_InvalidGeoData))
            {
                throw;
            }
        }
    }

    public virtual async Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        // The distance must be converted to radian (in contrast to MongoDB, which uses degrees).
        var radian = query.Radius / 6378100;

        // Use the filter in the correct order to leverage the index in the best way.
        var findFilter =
            Filter.And(
                Filter.Eq(x => x.AppId, app.Id),
                Filter.Eq(x => x.SchemaId, query.SchemaId),
                Filter.Eq(x => x.GeoField, query.Field),
                Filter.GeoWithinCenterSphere(x => x.GeoObject, query.Longitude, query.Latitude, radian),
                FilterByScope(scope));

        var byGeo =
            await GetCollection(scope).Find(findFilter).Limit(query.Take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId))
                .ToListAsync(ct);

        return byGeo.Select(x => x.ContentId).ToList();
    }

    public abstract Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default);

    protected static FilterDefinition<MongoTextIndexEntity<T>> FilterByScope(SearchScope scope)
    {
        if (scope == SearchScope.All)
        {
            return Filter.Eq(x => x.ServeAll, true);
        }
        else
        {
            return Filter.Eq(x => x.ServePublished, true);
        }
    }

    protected IMongoCollection<MongoTextIndexEntity<T>> GetCollection(SearchScope scope)
    {
        if (scope == SearchScope.All)
        {
            return Collection;
        }
        else
        {
            return Collection.WithReadPreference(ReadPreference.Secondary);
        }
    }
}
