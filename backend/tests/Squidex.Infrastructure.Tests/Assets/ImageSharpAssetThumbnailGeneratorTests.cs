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
        private readonly ImageSharpAssetThumbnailGenerator sut = new ImageSharpAssetThumbnailGenerator();
        private readonly MemoryStream target = new MemoryStream();

        [Fact]
        public async Task Should_return_same_image_if_no_size_and_quality_is_passed_for_thumbnail()
        {
            var source = GetPng();

            await sut.CreateThumbnailAsync(source, target, new ResizeOptions());

            Assert.Equal(target.Length, source.Length);
        }

        [Fact]
        public async Task Should_resize_image_to_target()
        {
            var source = GetPng();

            var options = new ResizeOptions { Width = 1000, Height = 1000, Mode = ResizeMode.BoxPad };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length > source.Length);
        }

        [Fact]
        public async Task Should_change_jpeg_quality_and_write_to_target()
        {
            var source = GetJpeg();

            var options = new ResizeOptions { Quality = 10 };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length < source.Length);
        }

        [Fact]
        public async Task Should_change_png_quality_and_write_to_target()
        {
            var source = GetPng();

            var options = new ResizeOptions { Quality = 10 };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length < source.Length);
        }

        [Fact]
        public async Task Should_return_image_information_if_image_is_valid()
        {
            var source = GetPng();

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Equal(600, imageInfo!.PixelHeight);
            Assert.Equal(600, imageInfo!.PixelWidth);
        }

        [Fact]
        public async Task Should_return_null_if_stream_is_not_an_image()
        {
            var source = new MemoryStream(Convert.FromBase64String("YXNkc2Fk"));

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Null(imageInfo);
        }

        private Stream GetPng()
        {
            return GetType().Assembly.GetManifestResourceStream("Squidex.Infrastructure.Assets.Images.logo.png")!;
        }

        private Stream GetJpeg()
        {
            return GetType().Assembly.GetManifestResourceStream("Squidex.Infrastructure.Assets.Images.logo.jpg")!;
        }
    }
}
