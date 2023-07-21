// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class SearchTests : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; }

    public SearchTests(ContentFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_search_asset()
    {
        // STEP 1: Create asset.
        await _.Client.Assets.UploadFileAsync("Assets/logo-wide.png", "image/png");


        // STEP 2: Search for schema.
        var result = await _.Client.Search.GetSearchResultsAsync("logo");

        Assert.Contains(result, x => x.Type == SearchResultType.Asset);
    }

    [Fact]
    public async Task Should_search_schema()
    {
        // STEP 1: Create schema.
        var schemaName = Guid.NewGuid().ToString();

        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        await _.Client.Schemas.PostSchemaAsync(createRequest);


        // STEP 2: Search for schema.
        var result = await _.Client.Search.PollAsync(schemaName, x => x.Type == SearchResultType.Content);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_search_content()
    {
        // STEP 1: Create content.
        var contentString = Guid.NewGuid().ToString();

        var createRequest = new TestEntityData
        {
            String = contentString
        };

        await _.Contents.CreateAsync(createRequest, ContentCreateOptions.AsPublish);


        // STEP 2: Search for schema.
        var result = await _.Client.Search.PollAsync(contentString, x => x.Type == SearchResultType.Content);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(SearchResultType.Asset, "Assets")]
    [InlineData(SearchResultType.Dashboard, "Dashboard")]
    [InlineData(SearchResultType.Rule, "Rules")]
    [InlineData(SearchResultType.Schema, "Schemas")]
    [InlineData(SearchResultType.Setting, "Backups")]
    [InlineData(SearchResultType.Setting, "Clients")]
    [InlineData(SearchResultType.Setting, "Contributors")]
    [InlineData(SearchResultType.Setting, "Languages")]
    [InlineData(SearchResultType.Setting, "Roles")]
    [InlineData(SearchResultType.Setting, "Subscription")]
    [InlineData(SearchResultType.Setting, "Workflows")]
    public async Task Should_search_for_dashboard_pages(SearchResultType expectedType, string query)
    {
        var result = await _.Client.Search.GetSearchResultsAsync(query);

        Assert.Contains(result, x => x.Type == expectedType);
    }
}
