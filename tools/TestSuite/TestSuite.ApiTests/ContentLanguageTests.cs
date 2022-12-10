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

public class ContentLanguageTests : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; }

    public ContentLanguageTests(ContentFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_filter_language()
    {
        // STEP 1: Create content
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = new Dictionary<string, string>
            {
                ["de"] = "Hallo",
                ["en"] = "Hello"
            }
        }, ContentCreateOptions.AsPublish, QueryContext.Default.WithLanguages("de"));

        Assert.False(content.Data.Localized.ContainsKey("en"));
        Assert.Equal("Hallo", content.Data.Localized["de"]);
    }

    [Theory]
    [InlineData("de", "Hallo")]
    [InlineData("en", "Hello")]
    [InlineData("custom", "Custom")]
    public async Task Should_flatten_language(string code, string expected)
    {
        // STEP 1: Create content
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = new Dictionary<string, string>
            {
                ["de"] = "Hallo",
                ["en"] = "Hello",
                ["custom"] = "Custom"
            }
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get content.
        var contents = _.ClientManager.CreateDynamicContentsClient(_.Contents.SchemaName);

        var contentFlatten = await contents.GetAsync(content.Id, QueryContext.Default.Flatten().WithLanguages(code));

        Assert.Equal(expected, (string)contentFlatten.Data["localized"]);
    }

    [Fact]
    public async Task Should_provide_etag_based_on_headers()
    {
        // STEP 1: Create content
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = new Dictionary<string, string>
            {
                ["de"] = "Hallo",
                ["en"] = "Hello"
            }
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get content.
        var (etag1, _) = await GetEtagAsync(content.Id, new Dictionary<string, string>());

        var (etag2, _) = await GetEtagAsync(content.Id, new Dictionary<string, string>
        {
            ["X-Flatten"] = "1"
        });

        var (etag3, _) = await GetEtagAsync(content.Id, new Dictionary<string, string>
        {
            ["X-Languages"] = "en"
        });

        var (etag4, _) = await GetEtagAsync(content.Id, new Dictionary<string, string>
        {
            ["X-Languages"] = "en",
            ["X-Flatten"] = "1"
        });

        static void AssertValue(string value, string not = null)
        {
            Assert.NotNull(value);
            Assert.NotEmpty(value);
            Assert.NotEqual(not, value);
        }

        AssertValue(etag1);
        AssertValue(etag2, etag1);
        AssertValue(etag3, etag1);
        AssertValue(etag4, etag1);
    }

    private async Task<(string, string)> GetEtagAsync(string id, Dictionary<string, string> headers)
    {
        var url = $"{_.ClientManager.Options.Url}api/content/{_.AppName}/{_.SchemaName}/{id}";

        using (var httpClient = _.ClientManager.CreateHttpClient())
        {
            foreach (var (key, value) in headers)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }

            var response = await httpClient.GetAsync(url);

            return (response.Headers.GetValues("ETag").FirstOrDefault(), response.Headers.Vary.ToString());
        }
    }
}
