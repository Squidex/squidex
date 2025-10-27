// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class DocumentDbTextIndex(IMongoDatabase database, string shardKey)
    : MongoTextIndexBase<string>(database, shardKey, new CommandFactory<string>(BuildTexts))
{
    private record struct SearchOperation
    {
        required public App App { get; init; }

        required public List<(DomainId Id, double Score)> Results { get; init; }

        required public string SearchTerms { get; init; }

        required public int Take { get; set; }

        required public SearchScope SearchScope { get; init; }
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<string>> collection,
        CancellationToken ct)
    {
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoTextIndexEntity<string>>(
                Index.Text(x => x.Texts)),
            cancellationToken: ct);


        await collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoTextIndexEntity<string>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ContentId)),

                new CreateIndexModel<MongoTextIndexEntity<string>>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.SchemaId)
                        .Ascending(x => x.GeoField)
                        .Geo2DSphere(x => x.GeoObject)),
            ], ct);
    }

    public override async Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        var point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(query.Longitude, query.Latitude));

        // Use the filter in the correct order to leverage the index in the best way.
        var findFilter =
            Filter.And(
                Filter.Eq(x => x.AppId, app.Id),
                Filter.Eq(x => x.SchemaId, query.SchemaId),
                Filter.Eq(x => x.GeoField, query.Field),
                Filter.NearSphere(x => x.GeoObject, point, query.Radius),
                FilterByScope(scope));

        var byGeo =
            await GetCollection(scope).Find(findFilter).Limit(query.Take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId))
                .ToListAsync(ct);

        return byGeo.Select(x => x.ContentId).ToList();
    }

    public override async Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return null;
        }

        // Use a custom tokenizer to leverage stop words from multiple languages.
        var search = new SearchOperation
        {
            App = app,
            SearchTerms = Tokenizer.Query(query.Text),
            SearchScope = scope,
            Results = [],
            Take = query.Take,
        };

        if (query.RequiredSchemaIds?.Count > 0)
        {
            await SearchBySchemaAsync(search, query.RequiredSchemaIds, 1, ct);
        }
        else if (query.PreferredSchemaId == null)
        {
            await SearchByAppAsync(search, 1, ct);
        }
        else
        {
            // We cannot write queries that prefer results from the same schema, therefore make two queries.
            search.Take /= 2;

            // Increasing the scoring of the results from the schema by 10 percent.
            await SearchBySchemaAsync(search, Enumerable.Repeat(query.PreferredSchemaId.Value, 1), 1.1, ct);
            await SearchByAppAsync(search, 1, ct);
        }

        return search.Results.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
    }

    private Task SearchBySchemaAsync(SearchOperation search, IEnumerable<DomainId> schemaIds, double factor,
        CancellationToken ct)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms),
                Filter.In(x => x.SchemaId, schemaIds),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, factor, ct);
    }

    private Task SearchByAppAsync(SearchOperation search, double factor,
        CancellationToken ct)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, factor, ct);
    }

    private async Task SearchAsync(SearchOperation search, FilterDefinition<MongoTextIndexEntity<string>> filter, double factor,
        CancellationToken ct)
    {
        var byText =
            await GetCollection(search.SearchScope).Find(filter).Limit(search.Take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId).MetaTextScore("score")).Sort(Sort.MetaTextScore("score"))
                .ToListAsync(ct);

        search.Results.AddRange(byText.Select(x => (x.ContentId, x.Score * factor)));
    }

    private static string BuildTexts(Dictionary<string, string> source)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            foreach (var (key, value) in source)
            {
                sb.Append(' ');
                sb.Append(Tokenizer.Terms(value, key));
            }

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }
}
