// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class AssetTests : IClassFixture<AssetFixture>
    {
        public AssetFixture _ { get; }

        public AssetTests(AssetFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_replace_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Reupload asset
            var asset_2 = await _.UploadFileAsync("Assets/logo-wide.png", asset_1);

            using (var stream = new FileStream("Assets/logo-wide.png", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(asset_2);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }
        }

        [Fact]
        public async Task Should_annote_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Annotate metadata.
            var metadataRequest = new AnnotateAssetDto
            {
                Metadata = new Dictionary<string, object>
                {
                    ["pw"] = 100L,
                    ["ph"] = 20L
                }
            };

            var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id.ToString(), metadataRequest);

            // Should provide metadata.
            Assert.Equal(metadataRequest.Metadata, asset_2.Metadata);


            // STEP 3: Annotate slug.
            var slugRequest = new AnnotateAssetDto { Slug = "my-image" };

            var asset_3 = await _.Assets.PutAssetAsync(_.AppName, asset_2.Id.ToString(), slugRequest);

            // Should provide updated slug.
            Assert.Equal(slugRequest.Slug, asset_3.Slug);


            // STEP 3: Annotate file name.
            var fileNameRequest = new AnnotateAssetDto { FileName = "My Image" };

            var asset_4 = await _.Assets.PutAssetAsync(_.AppName, asset_3.Id.ToString(), fileNameRequest);

            // Should provide updated file name.
            Assert.Equal(fileNameRequest.FileName, asset_4.FileName);
        }

        [Fact]
        public async Task Should_protect_asset()
        {
            var fileName = $"{Guid.NewGuid()}.png";

            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Download asset
            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var downloaded = await _.DownloadAsync(asset_1);

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }


            // STEP 4: Protect asset
            var protectRequest = new AnnotateAssetDto { IsProtected = true };

            var asset_2 = await _.Assets.PutAssetAsync(_.AppName, asset_1.Id.ToString(), protectRequest);


            // STEP 5: Download asset with authentication.
            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var downloaded = new MemoryStream();

                using (var assetStream = await _.Assets.GetAssetContentAsync(asset_2.Id.ToString()))
                {
                    await assetStream.Stream.CopyToAsync(downloaded);
                }

                // Should dowload with correct size.
                Assert.Equal(stream.Length, downloaded.Length);
            }


            // STEP 5: Download asset without key.
            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _.DownloadAsync(asset_1));

                // Should return 403 when not authenticated.
                Assert.Contains("403", ex.Message);
            }
        }

        [Fact]
        public async Task Should_delete_asset()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Delete asset
            await _.Assets.DeleteAssetAsync(_.AppName, asset_1.Id.ToString());

            // Should return 404 when asset deleted.
            var ex = await Assert.ThrowsAsync<SquidexManagementException>(() => _.Assets.GetAssetAsync(_.AppName, asset_1.Id.ToString()));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task Should_query_asset_by_metadata()
        {
            // STEP 1: Create asset
            var asset_1 = await _.UploadFileAsync("Assets/logo-squared.png", "image/png");


            // STEP 2: Query asset
            var assets = await _.Assets.GetAssetsAsync(_.AppName, new AssetQuery
            {
                Filter = "metadata/pixelWidth eq 600"
            });

            Assert.Contains(assets.Items, x => x.Id == asset_1.Id);
        }
    }
}
