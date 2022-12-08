// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Text.Azure;

public sealed class AzureTextIndex : IInitializable, ITextIndex
{
    private readonly SearchIndexClient indexClient;
    private readonly SearchClient searchClient;
    private readonly QueryParser queryParser = new QueryParser(AzureIndexDefinition.GetFieldName);

    public AzureTextIndex(
        string serviceEndpoint,
        string serviceApiKey,
        string indexName)
    {
        indexClient = new SearchIndexClient(new Uri(serviceEndpoint), new AzureKeyCredential(serviceApiKey));

        searchClient = indexClient.GetSearchClient(indexName);
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await CreateIndexAsync(ct);
    }

    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        await indexClient.DeleteIndexAsync(searchClient.IndexName, ct);

        await CreateIndexAsync(ct);
    }

    private async Task CreateIndexAsync(
        CancellationToken ct)
    {
        var index = AzureIndexDefinition.Create(searchClient.IndexName);

        await indexClient.CreateOrUpdateIndexAsync(index, true, true, ct);
    }

    public async Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        var batch = IndexDocumentsBatch.Create<SearchDocument>();

        commands.Foreach(x => CommandFactory.CreateCommands(x, batch.Actions));

        if (batch.Actions.Count == 0)
        {
            return;
        }

        await searchClient.IndexDocumentsAsync(batch, cancellationToken: ct);
    }

    public async Task<List<DomainId>> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        var result = new List<(DomainId Id, double Score)>();

        await SearchAsync(result, "*", BuildGeoQuery(query, scope), query.Take, 1, ct);

        return result.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
    }

    public async Task<List<DomainId>> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        var parsed = queryParser.Parse(query.Text);

        if (parsed == null)
        {
            return null;
        }

        var result = new List<(DomainId Id, double Score)>();

        if (query.RequiredSchemaIds?.Count > 0)
        {
            await SearchBySchemaAsync(result, parsed.Text, query.RequiredSchemaIds, scope, query.Take, 1, ct);
        }
        else if (query.PreferredSchemaId == null)
        {
            await SearchByAppAsync(result, parsed.Text, app, scope, query.Take, 1, ct);
        }
        else
        {
            var halfTake = query.Take / 2;

            var schemaIds = Enumerable.Repeat(query.PreferredSchemaId.Value, 1);

            await SearchBySchemaAsync(result, parsed.Text, schemaIds, scope, halfTake, 1.1, ct);
            await SearchByAppAsync(result, parsed.Text, app, scope, halfTake, 1, ct);
        }

        return result.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
    }

    private Task SearchBySchemaAsync(List<(DomainId, double)> result, string text, IEnumerable<DomainId> schemaIds, SearchScope scope, int take, double factor,
        CancellationToken ct = default)
    {
        var searchField = GetServeField(scope);

        var filter = $"{string.Join(" or ", schemaIds.Select(x => $"schemaId eq '{x}'"))} and {searchField} eq true";

        return SearchAsync(result, text, filter, take, factor, ct);
    }

    private Task SearchByAppAsync(List<(DomainId, double)> result, string text, IAppEntity app, SearchScope scope, int take, double factor,
        CancellationToken ct = default)
    {
        var searchField = GetServeField(scope);

        var filter = $"appId eq '{app.Id}' and {searchField} eq true";

        return SearchAsync(result, text, filter, take, factor, ct);
    }

    private async Task SearchAsync(List<(DomainId, double)> result, string text, string filter, int take, double factor,
        CancellationToken ct = default)
    {
        var searchOptions = new SearchOptions
        {
            Filter = filter
        };

        searchOptions.Select.Add("contentId");
        searchOptions.Size = take;
        searchOptions.QueryType = SearchQueryType.Full;

        var results = await searchClient.SearchAsync<SearchDocument>(text, searchOptions, ct);

        await foreach (var item in results.Value.GetResultsAsync().WithCancellation(ct))
        {
            if (item != null)
            {
                var id = DomainId.Create(item.Document["contentId"].ToString());

                result.Add((id, factor * item.Score ?? 0));
            }
        }
    }

    private static string BuildGeoQuery(GeoQuery query, SearchScope scope)
    {
        var (schema, field, lat, lng, radius, _) = query;

        var searchField = GetServeField(scope);
        var searchDistance = radius / 1000;

        return $"schemaId eq '{schema}' and geoField eq '{field}' and geo.distance(geoObject, geography'POINT({lng} {lat})') lt {searchDistance} and {searchField} eq true";
    }

    private static string GetServeField(SearchScope scope)
    {
        return scope == SearchScope.Published ?
            "servePublished" :
            "serveAll";
    }
}
