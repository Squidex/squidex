// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoTextIndex : MongoTextIndexBase<List<MongoTextIndexEntityText>>
{
    private record struct SearchOperation
    {
        required public App App { get; init; }

        required public List<(DomainId Id, double Score)> Results { get; init; }

        required public string SearchTerms { get; init; }

        required public int Take { get; set; }

        required public SearchScope SearchScope { get; init; }
    }

    public MongoTextIndex(IMongoDatabase database, string shardKey)
        : base(database, shardKey, new CommandFactory<List<MongoTextIndexEntityText>>(BuildTexts))
    {
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity<List<MongoTextIndexEntityText>>> collection,
        CancellationToken ct)
    {
        await base.SetupCollectionAsync(collection, ct);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoTextIndexEntity<List<MongoTextIndexEntityText>>>(
                Index
                    .Ascending(x => x.AppId)
                    .Text("t.t")),
            cancellationToken: ct);
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
            Take = query.Take
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
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms, "none"),
                Filter.In(x => x.SchemaId, schemaIds),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, factor, ct);
    }

    private Task SearchByAppAsync(SearchOperation search, double factor,
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms, "none"),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, factor, ct);
    }

    private async Task SearchAsync(SearchOperation search, FilterDefinition<MongoTextIndexEntity<List<MongoTextIndexEntityText>>> filter, double factor,
        CancellationToken ct = default)
    {
        var byText =
            await GetCollection(search.SearchScope).Find(filter).Limit(search.Take)
                .Project<MongoTextResult>(Projection.Include(x => x.ContentId).MetaTextScore("score")).Sort(Sort.MetaTextScore("score"))
                .ToListAsync(ct);

        search.Results.AddRange(byText.Select(x => (x.ContentId, x.Score * factor)));
    }

    private static List<MongoTextIndexEntityText> BuildTexts(Dictionary<string, string> source)
    {
        // Use a custom tokenizer to leverage stop words from multiple languages.
        return source.Select(x => MongoTextIndexEntityText.FromText(Tokenizer.Terms(x.Value, x.Key))).ToList();
    }
}
