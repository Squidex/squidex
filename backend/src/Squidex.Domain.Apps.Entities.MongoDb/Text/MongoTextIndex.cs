// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoTextIndex : MongoTextIndexBase<List<MongoTextIndexEntityText>>
{
    private record struct SearchOperation
    {
        required public App App { get; init; }

        required public string SearchTerms { get; set; }

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

        // In compound indexes you have to use equality for all prefix keys, therefore we cannot add the schema.
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoTextIndexEntity<List<MongoTextIndexEntityText>>>(
                Index
                    .Ascending(x => x.AppId)
                    .Text("t.t")
                    .Text(x => x.SchemaId),
                new CreateIndexOptions
                {
                    Weights = new BsonDocument
                    {
                        ["t.t"] = 2,
                        ["s"] = 1
                    }
                }),
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
            Take = query.Take
        };

        if (query.RequiredSchemaIds?.Count > 0)
        {
            return await SearchBySchemaAsync(search, query.RequiredSchemaIds, ct);
        }
        else if (query.PreferredSchemaId == null)
        {
            return await SearchByAppAsync(search, ct);
        }
        else
        {
            // Also include the schema Id as additional search term to prefer that.
            search.SearchTerms = $"{query.PreferredSchemaId} {search.SearchTerms}";

            return await SearchByAppAsync(search, ct);
        }
    }

    private Task<List<DomainId>> SearchBySchemaAsync(SearchOperation search, IEnumerable<DomainId> schemaIds,
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms, "none"),
                Filter.In(x => x.SchemaId, schemaIds),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, ct);
    }

    private Task<List<DomainId>> SearchByAppAsync(SearchOperation search,
        CancellationToken ct = default)
    {
        var filter =
            Filter.And(
                Filter.Eq(x => x.AppId, search.App.Id),
                Filter.Text(search.SearchTerms, "none"),
                FilterByScope(search.SearchScope));

        return SearchAsync(search, filter, ct);
    }

    private async Task<List<DomainId>> SearchAsync(SearchOperation search, FilterDefinition<MongoTextIndexEntity<List<MongoTextIndexEntityText>>> filter,
        CancellationToken ct = default)
    {
        var byText =
            await GetCollection(search.SearchScope).Find(filter).Limit(search.Take).Only(x => x.ContentId).Sort(Sort.MetaTextScore("score"))
                .ToListAsync(ct);

        return byText.Select(x => DomainId.Create(x["c"].AsString)).ToList();
    }

    private static List<MongoTextIndexEntityText> BuildTexts(Dictionary<string, string> source)
    {
        // Use a custom tokenizer to leverage stop words from multiple languages.
        return source.Select(x => MongoTextIndexEntityText.FromText(Tokenizer.Terms(x.Value, x.Key))).ToList();
    }
}
