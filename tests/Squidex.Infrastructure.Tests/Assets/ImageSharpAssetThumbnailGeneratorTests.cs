// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.Assets.ImageSharp;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class ImageSharpAssetThumbnailGeneratorTests
    {
        private const string Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMTM0A1t6AAAADElEQVQYV2P4//8/AAX+Av6nNYGEAAAAAElFTkSuQmCC";
        private readonly ImageSharpAssetThumbnailGenerator sut = new ImageSharpAssetThumbnailGenerator();

        [Fact]
        public async Task Should_return_same_image_if_no_size_is_passed_for_thumbnail()
        {
            var source = new MemoryStream(Convert.FromBase64String(Image));
            var target = new MemoryStream();

            await sut.CreateThumbnailAsync(source, target, null, null, "resize");

            Assert.Equal(target.Length, source.Length);
        }

        [Fact]
        public async Task Should_resize_image_to_target()
        {
            var source = new MemoryStream(Convert.FromBase64String(Image));
            var target = new MemoryStream();

            await sut.CreateThumbnailAsync(source, target, 100, 100, "resize");

            Assert.True(target.Length > source.Length);
        }

        [Fact]
        public async Task Should_return_image_information_if_image_is_valid()
        {
            var source = new MemoryStream(Convert.FromBase64String(Image));

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Equal(1, imageInfo.PixelHeight);
            Assert.Equal(1, imageInfo.PixelWidth);
        }

        [Fact]
        public async Task Should_return_null_if_stream_is_not_an_image()
        {
            var source = new MemoryStream(Convert.FromBase64String("YXNkc2Fk"));

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Null(imageInfo);
        }
    }
}
