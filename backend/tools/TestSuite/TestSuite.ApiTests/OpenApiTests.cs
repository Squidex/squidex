// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class OpenApiTests : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; }

    public OpenApiTests(ContentFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_provide_general_spec()
    {
        var url = $"{_.ClientManager.Options.Url}api/swagger/v1/swagger.json";

        var document = await OpenApiDocument.FromUrlAsync(url);

        Assert.NotNull(document);
    }

    [Fact]
    public async Task Should_provide_content_spec()
    {
        var url = $"{_.ClientManager.Options.Url}api/content/{_.AppName}/swagger/v1/swagger.json";

        var document = await OpenApiDocument.FromUrlAsync(url);

        Assert.NotNull(document);
    }

    [Fact]
    public async Task Should_provide_flat_content_spec()
    {
        var url = $"{_.ClientManager.Options.Url}api/content/{_.AppName}/flat/swagger/v1/swagger.json";

        var document = await OpenApiDocument.FromUrlAsync(url);

        Assert.NotNull(document);
    }
}
