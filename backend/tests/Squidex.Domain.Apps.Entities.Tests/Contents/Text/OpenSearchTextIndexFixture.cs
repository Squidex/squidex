// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Extensions.Text.ElasticSearch;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class OpenSearchTextIndexFixture : IAsyncLifetime
{
    public ElasticSearchTextIndex Index { get; }

    public OpenSearchTextIndexFixture()
    {
        Index = new ElasticSearchTextIndex(
            new OpenSearchClient(TestConfig.Configuration["elastic:configuration"]),
            TestConfig.Configuration["elastic:indexName"],
            TestUtils.DefaultSerializer);
    }

    public Task InitializeAsync()
    {
        return Index.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
