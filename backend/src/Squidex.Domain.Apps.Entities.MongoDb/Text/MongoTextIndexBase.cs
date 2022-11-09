// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public abstract class MongoTextIndexBase<T> : MongoRepositoryBase<MongoTextIndexEntity<T>>, ITextIndex, IDeleter where T : class
{
    private readonly CommandFactory<T> commandFactory;

    protected sealed class MongoTextResult
    {
        [BsonId]
        [BsonElement]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement("_ci")]
        public DomainId ContentId { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("score")]
        public double Score { get; set; }
    }

    protected MongoTextIndexBase(IMongoDatabase database)
        : base(database)
    {
#pragma warning disable MA0056 // Do not call overridable members in constructor
        commandFactory = new CommandFactory<T>(BuildTexts);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<T>> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoTextIndexEntity<T>>(
                Index.Ascending(x => x.DocId)),

            new CreateIndexModel<MongoTextIndexEntity<T>>(
                Index
                    .Ascending(x => x.AppId)
                    .Ascending(x => x.ServeAll)
                    .Ascending(x => x.ServePublished)
                    .Ascending(x => x.SchemaId)
                    .Ascending(x => x.GeoField)
                    .Geo2DSphere(x => x.GeoObject))
        }, ct);
    }

    protected override string CollectionName()
    {
        return "TextIndex";
    }

    protected abstract T BuildTexts(Dictionary<string, string> source);

    async Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        await Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
    }

    public async virtual Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        var writes = new List<WriteModel<MongoTextIndexEntity<T>>>(commands.Length);

        foreach (var command in commands)
        {
            commandFactory.CreateCommands(command, writes);
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
            // Ignore invalid geo data.
            if (ex.WriteErrors.Any(error => error.Code != MongoDbErrorCodes.Errror16755_InvalidGeoData))
            {
                throw;
            }
        }
    }

    public virtual async Task<List<DomainId>?> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        var findFilter =
            Filter.And(
                Filter.Eq(x => x.AppId, app.Id),
                Filter.Eq(x => x.SchemaId, query.SchemaId),
                Filter_ByScope(scope),
                Filter.GeoWithinCenterSphere(x => x.GeoObject, query.Longitude, query.Latitude, query.Radius / 6378100));

        var byGeo =
            await GetCollection(scope).Find(findFilter).Limit(query.Take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId))
                .ToListAsync(ct);

        return byGeo.Select(x => x.ContentId).ToList();
    }

    public virtual async Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        var (search, take) = query;

        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var result = new List<(DomainId Id, double Score)>();

        if (query.RequiredSchemaIds?.Count > 0)
        {
            await SearchBySchemaAsync(result, search, app, query.RequiredSchemaIds, scope, take, 1, ct);
        }
        else if (query.PreferredSchemaId == null)
        {
            await SearchByAppAsync(result, search, app, scope, take, 1, ct);
        }
        else
        {
            var halfBucket = take / 2;

            var schemaIds = Enumerable.Repeat(query.PreferredSchemaId.Value, 1);

            await SearchBySchemaAsync(result, search, app, schemaIds, scope, halfBucket, 1.1, ct);
            await SearchByAppAsync(result, search, app, scope, halfBucket, 1, ct);
        }

        return result.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
    }

    private Task SearchBySchemaAsync(List<(DomainId, double)> result, string text, IAppEntity app, IEnumerable<DomainId> schemaIds, SearchScope scope, int take, double factor,
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, app.Id),
                Filter.In(x => x.SchemaId, schemaIds),
                Filter_ByScope(scope),
                Filter.Text(text, "none"));

        return SearchAsync(result, filter, scope, take, factor, ct);
    }

    private Task SearchByAppAsync(List<(DomainId, double)> result, string text, IAppEntity app, SearchScope scope, int take, double factor,
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, app.Id),
                Filter.Exists(x => x.SchemaId),
                Filter_ByScope(scope),
                Filter.Text(text, "none"));

        return SearchAsync(result, filter, scope, take, factor, ct);
    }

    private async Task SearchAsync(List<(DomainId, double)> result, FilterDefinition<MongoTextIndexEntity<T>> filter, SearchScope scope, int take, double factor,
        CancellationToken ct = default)
    {
        var byText =
            await GetCollection(scope).Find(filter).Limit(take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId).MetaTextScore("score")).Sort(Sort.MetaTextScore("score"))
                .ToListAsync(ct);

        result.AddRange(byText.Select(x => (x.ContentId, x.Score * factor)));
    }

    private static FilterDefinition<MongoTextIndexEntity<T>> Filter_ByScope(SearchScope scope)
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

    private IMongoCollection<MongoTextIndexEntity<T>> GetCollection(SearchScope scope)
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
