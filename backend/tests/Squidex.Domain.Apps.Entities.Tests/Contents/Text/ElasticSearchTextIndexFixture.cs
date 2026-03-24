// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Extensions.Text.ElasticSearch;
using Testcontainers.Elasticsearch;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class ElasticSearchTextIndexFixture : IAsyncLifetime
{
    private readonly ElasticsearchContainer elastic =
        new ElasticsearchBuilder("elasticsearch:8.6.1")
            .WithReuse(true)
            .WithLabel("resuse-id", "elastic-text")
            .Build();

    public ElasticSearchTextIndex Index { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await elastic.StartAsync(TestContext.Current.CancellationToken);

        Index = new ElasticSearchTextIndex(
            new ElasticSearchClient(elastic.GetConnectionString()),
            TestConfig.Configuration["elastic:indexName"]!,
            TestUtils.DefaultSerializer);

        await Index.InitializeAsync(default);
    }

    public async ValueTask DisposeAsync()
    {
        await elastic.StopAsync(TestContext.Current.CancellationToken);
    }
}
