// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Http.Headers;
using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class CachingTests(ContentFixture fixture) : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_return_not_modified_for_content()
    {
        var content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, ContentCreateOptions.AsPublish);

        var url = $"{_.Client.Options.Url}api/content/{_.AppName}/{_.SchemaName}/{content.Id}";

        using var httpClient = _.Client.CreateHttpClient();

        // STEP 1: Get the content with the etag.
        var response_1 = await httpClient.GetAsync(url);

        var etag = response_1.Headers.ETag;

        Assert.Equal(HttpStatusCode.OK, response_1.StatusCode);
        Assert.NotNull(etag);


        // STEP 2: Get the content again with the etag.
        using var request_1 = new HttpRequestMessage(HttpMethod.Get, url);

        request_1.Headers.IfNoneMatch.Add(etag);

        var response_2 = await httpClient.SendAsync(request_1);

        Assert.Equal(HttpStatusCode.NotModified, response_2.StatusCode);


        // STEP 3: Update the content and use the old etag again.
        await _.Contents.UpdateAsync(content.Id, new TestEntityData { Number = 2 });

        using var request_2 = new HttpRequestMessage(HttpMethod.Get, url);

        request_2.Headers.IfNoneMatch.Add(etag);

        var response_3 = await httpClient.SendAsync(request_2);

        Assert.Equal(HttpStatusCode.OK, response_3.StatusCode);
    }

    [Fact]
    public async Task Should_limit_cache_duration_of_asset()
    {
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");

        // STEP 1: Download the asset with a cache duration.
        var cacheControl_1 = await GetCacheControlAsync(asset.Id, 60);

        Assert.True(cacheControl_1?.Public);
        Assert.Equal(TimeSpan.FromSeconds(60), cacheControl_1?.MaxAge);


        // STEP 2: Download the asset with a very long cache duration.
        var cacheControl_2 = await GetCacheControlAsync(asset.Id, int.MaxValue);

        // The cache duration is limited by the configuration of the server.
        Assert.True(cacheControl_2?.MaxAge < TimeSpan.FromSeconds(int.MaxValue));
    }

    [Fact]
    public async Task Should_not_cache_protected_asset_publicly()
    {
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");

        // STEP 1: Protect the asset.
        var protectRequest = new AnnotateAssetDto
        {
            IsProtected = true,
        };

        await _.Client.Assets.PutAssetAsync(asset.Id, protectRequest);


        // STEP 2: Download the asset with a cache duration.
        var cacheControl = await GetCacheControlAsync(asset.Id, 60);

        // A shared cache must not store a protected asset, because the url has no credentials.
        Assert.False(cacheControl?.Public);
        Assert.True(cacheControl?.NoStore);
    }

    private async Task<CacheControlHeaderValue?> GetCacheControlAsync(string id, long cacheDuration)
    {
        var url = $"{_.Client.Options.Url}api/assets/{_.AppName}/{id}?cache={cacheDuration}";

        using var httpClient = _.Client.CreateHttpClient();

        var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return response.Headers.CacheControl;
    }
}
