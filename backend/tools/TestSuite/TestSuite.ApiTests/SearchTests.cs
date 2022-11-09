// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
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
        // STEP 1: Create asset
        await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-wide.png", "image/png");


        // STEP 2: Search for schema.
        var result = await _.Search.GetSearchResultsAsync(_.AppName, "logo");

        Assert.Contains(result, x => x.Type == SearchResultType.Asset);
    }

    [Fact]
    public async Task Should_search_schema()
    {
        // STEP 1: Create schema
        var schemaName = Guid.NewGuid().ToString();

        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        await _.Schemas.PostSchemaAsync(_.AppName, createRequest);


        // STEP 2: Search for schema.
        var result = await _.Search.WaitForSearchAsync(_.AppName, schemaName, x => x.Type == SearchResultType.Content, TimeSpan.FromSeconds(30));

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Should_search_content()
    {
        // STEP 1: Create content
        var contentString = Guid.NewGuid().ToString();

        var createRequest = new TestEntityData
        {
            String = contentString
        };

        await _.Contents.CreateAsync(createRequest, ContentCreateOptions.AsPublish);


        // STEP 2: Search for schema.
        var result = await _.Search.WaitForSearchAsync(_.AppName, contentString, x => x.Type == SearchResultType.Content, TimeSpan.FromSeconds(30));

        Assert.NotEmpty(result);
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
        var result = await _.Search.GetSearchResultsAsync(_.AppName, query);

        Assert.Contains(result, x => x.Type == expectedType);
    }
}
