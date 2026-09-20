// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AssetResizeTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_return_same_variant_for_concurrent_requests()
    {
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-wide.png", "image/png");

        // Use a random size, so that the variant is not cached from another test.
        var width = Random.Shared.Next(100, 400);

        var url = $"{_.Client.Options.Url}api/assets/{_.AppName}/{asset.Id}?width={width}";

        // STEP 1: Download the same new variant in parallel.
        var downloads = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => DownloadAsync(url)));

        // All requests must return the same image, even though the variant is generated only once.
        Assert.All(downloads, x => Assert.NotEmpty(x));
        Assert.All(downloads, x => Assert.Equal(downloads[0], x));


        // STEP 2: Download the stored variant again.
        var downloaded = await DownloadAsync(url);

        Assert.Equal(downloads[0], downloaded);
    }

    private async Task<byte[]> DownloadAsync(string url)
    {
        using var httpClient = _.Client.CreateHttpClient();

        var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
