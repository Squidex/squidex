// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Assets.ImageSharp;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class ImageSharpAssetThumbnailGeneratorTests
    {
        private readonly ImageSharpAssetThumbnailGenerator sut = new ImageSharpAssetThumbnailGenerator();
        private readonly MemoryStream target = new MemoryStream();

        public static IEnumerable<object[]> GetConversions()
        {
            var allFormats = Enum.GetValues(typeof(ImageFormat)).OfType<ImageFormat>().Where(x => x != ImageFormat.Auto);

            foreach (var source in allFormats)
            {
                foreach (var target in allFormats)
                {
                    if (!Equals(target, source))
                    {
                        yield return new object[] { target, source };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetConversions))]
        public async Task Should_convert_between_formats(ImageFormat sourceFormat, ImageFormat targetFormat)
        {
            var source = GetImage(sourceFormat);

            var options = new ResizeOptions { Format = targetFormat };

            await sut.CreateThumbnailAsync(source, target, options);

            target.Position = 0;

            var imageInfo = await sut.GetImageInfoAsync(target);

            Assert.Equal(targetFormat.ToString(), imageInfo?.Format);
        }

        [Fact]
        public async Task Should_return_same_image_if_no_size_and_quality_is_passed_for_thumbnail()
        {
            var source = GetImage(ImageFormat.PNG);

            await sut.CreateThumbnailAsync(source, target, new ResizeOptions());

            Assert.Equal(target.Length, source.Length);
        }

        [Fact]
        public async Task Should_resize_image_to_target()
        {
            var source = GetImage(ImageFormat.PNG);

            var options = new ResizeOptions { Width = 1000, Height = 1000, Mode = ResizeMode.BoxPad };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length > source.Length);
        }

        [Fact]
        public async Task Should_change_jpeg_quality_and_write_to_target()
        {
            var source = GetImage(ImageFormat.JPEG);

            var options = new ResizeOptions { Quality = 10 };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length < source.Length);
        }

        [Fact]
        public async Task Should_change_png_quality_and_write_to_target()
        {
            var source = GetImage(ImageFormat.PNG);

            var options = new ResizeOptions { Quality = 10 };

            await sut.CreateThumbnailAsync(source, target, options);

            Assert.True(target.Length < source.Length);
        }

        [Fact]
        public async Task Should_auto_orient_image()
        {
            var source = GetRotatedJpeg();

            var imageInfo = await sut.FixOrientationAsync(source, target);

            Assert.Equal(135, imageInfo.PixelHeight);
            Assert.Equal(600, imageInfo.PixelWidth);

            Assert.False(imageInfo.IsRotatedOrSwapped);
        }

        [Fact]
        public async Task Should_return_image_information_if_image_is_valid()
        {
            var source = GetImage(ImageFormat.PNG);

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Equal(600, imageInfo!.PixelHeight);
            Assert.Equal(600, imageInfo!.PixelWidth);

            Assert.False(imageInfo.IsRotatedOrSwapped);
        }

        [Fact]
        public async Task Should_return_image_information_if_rotated()
        {
            var source = GetRotatedJpeg();

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Equal(600, imageInfo!.PixelHeight);
            Assert.Equal(135, imageInfo!.PixelWidth);

            Assert.True(imageInfo.IsRotatedOrSwapped);
        }

        [Fact]
        public async Task Should_return_null_if_stream_is_not_an_image()
        {
            var source = new MemoryStream(Convert.FromBase64String("YXNkc2Fk"));

            var imageInfo = await sut.GetImageInfoAsync(source);

            Assert.Null(imageInfo);
        }

        private Stream GetImage(ImageFormat format)
        {
            var name = $"Squidex.Infrastructure.Assets.Images.logo.{format.ToString().ToLowerInvariant()}";

            return GetType().Assembly.GetManifestResourceStream(name)!;
        }

        private Stream GetRotatedJpeg()
        {
            var name = "Squidex.Infrastructure.Assets.Images.logo-wide-rotated.jpg";

            return GetType().Assembly.GetManifestResourceStream(name)!;
        }
    }
}
