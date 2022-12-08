// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Extensions.Text.Azure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class AzureTextIndexFixture : IAsyncLifetime
{
    public AzureTextIndex Index { get; }

    public AzureTextIndexFixture()
    {
        Index = new AzureTextIndex(
            TestConfig.Configuration["azureText:serviceEndpoint"],
            TestConfig.Configuration["azureText:apiKey"],
            TestConfig.Configuration["azureText:indexName"]);
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
