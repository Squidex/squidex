// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenSearch.Net;

namespace Squidex.Extensions.Text.ElasticSearch;

public sealed class OpenSearchClient : IElasticSearchClient
{
    private readonly IOpenSearchLowLevelClient openSearch;

    public OpenSearchClient(string configurationString)
    {
        var config = new ConnectionConfiguration(new Uri(configurationString));

        openSearch = new OpenSearchLowLevelClient(config);
    }

    public async Task CreateIndexAsync<T>(string indexName, T request,
        CancellationToken ct)
    {
        var result = await openSearch.Indices.PutMappingAsync<StringResponse>(indexName, CreatePost(request), ctx: ct);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
        }
    }

    public async Task BulkAsync<T>(List<T> requests,
        CancellationToken ct)
    {
        var result = await openSearch.BulkAsync<StringResponse>(CreatePost(requests), ctx: ct);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
        }
    }

    public async Task<List<dynamic>> SearchAsync<T>(string indexName, T request,
        CancellationToken ct)
    {
        var result = await openSearch.SearchAsync<DynamicResponse>(indexName, CreatePost(request), ctx: ct);

        if (!result.Success)
        {
            throw result.OriginalException;
        }

        var hits = new List<dynamic>();

        foreach (var item in result.Body.hits.hits)
        {
            if (item != null)
            {
                hits.Add(item);
            }
        }

        return hits;
    }

    private static PostData CreatePost<T>(List<T> requests)
    {
        return PostData.MultiJson(requests.OfType<object>());
    }

    private static PostData CreatePost<T>(T data)
    {
        return new SerializableData<T>(data);
    }
}
