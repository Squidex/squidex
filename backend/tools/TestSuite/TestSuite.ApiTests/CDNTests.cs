﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;
using System.Threading.Tasks;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    [Trait("Category", "NotAutomated")]
    public class CDNTests : IClassFixture<CloudFixture>
    {
        public CloudFixture _ { get; }

        public CDNTests(CloudFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public void Should_provide_asset_url_from_cdn()
        {
            var id = "ef4286f9-8b1d-4dda-bd52-c5bd191c47bb";

            var url = _.CDNClientManager.GenerateImageUrl(id);

            Assert.StartsWith("https://assets.squidex.io/", url);
        }

        [Fact]
        public async Task Should_download_asset_url_from_cdn()
        {
            var id = "ef4286f9-8b1d-4dda-bd52-c5bd191c47bb";

            var url = _.CDNClientManager.GenerateImageUrl(id);

            await DownloadAsync(url);
        }

        [Fact]
        public void Should_provide_asset_url_from_cloud_when_cdn_not_configured()
        {
            var id = "ef4286f9-8b1d-4dda-bd52-c5bd191c47bb";

            var url = _.ClientManager.GenerateImageUrl(id);

            Assert.StartsWith("https://cloud.squidex.io/", url);
        }

        [Fact]
        public async Task Should_download_asset_url_from_cloud_when_cdn_not_configured()
        {
            var id = "ef4286f9-8b1d-4dda-bd52-c5bd191c47bb";

            var url = _.ClientManager.GenerateImageUrl(id);

            await DownloadAsync(url);
        }

        [Fact]
        public async Task Should_get_blog_items_from_cdn()
        {
            var client = _.CDNClientManager.CreateContentsClient<TestEntity, TestEntityData>("blog");

            var result = await client.GetAsync();

            Assert.NotEmpty(result.Items);
        }

        [Fact]
        public async Task Should_get_blog_items_from_cloud_when_cdn_not_configured()
        {
            var client = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>("blog");

            var result = await client.GetAsync();

            Assert.NotEmpty(result.Items);
        }

        private static async Task DownloadAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var buffer = await response.Content.ReadAsByteArrayAsync();

                Assert.True(buffer.Length > 1000);
            }
        }
    }
}
