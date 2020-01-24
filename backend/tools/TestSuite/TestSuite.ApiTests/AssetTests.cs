// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
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
        public async Task Should_upload_image()
        {
            var fileName = $"{Guid.NewGuid()}.png";

            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var asset = await _.Assets.CreateAssetAsync(fileName, "image/png", stream);

                Assert.True(asset.IsImage);
                Assert.Equal(600, asset.PixelHeight);
                Assert.Equal(600, asset.PixelWidth);
            }
        }

        [Fact]
        public async Task Should_upload_image_without_extension()
        {
            var fileName = $"{Guid.NewGuid()}.png";

            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var asset = await _.Assets.CreateAssetAsync(fileName, "image/png", stream);

                Assert.True(asset.IsImage);
                Assert.Equal(600, asset.PixelHeight);
                Assert.Equal(600, asset.PixelWidth);
            }
        }
    }
}
