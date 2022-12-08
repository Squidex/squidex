// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Extensions.Text.ElasticSearch;

public interface IElasticSearchClient
{
    Task CreateIndexAsync<T>(string indexName, T request,
        CancellationToken ct);

    Task BulkAsync<T>(List<T> requests,
        CancellationToken ct);

    Task<List<dynamic>> SearchAsync<T>(string indexName, T request,
        CancellationToken ct);
}
