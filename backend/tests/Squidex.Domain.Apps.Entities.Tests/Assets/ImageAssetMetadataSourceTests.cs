// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class ImageAssetMetadataSourceTests
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly MemoryStream stream = new MemoryStream();
        private readonly AssetFile file;
        private readonly ImageAssetMetadataSource sut;

        public ImageAssetMetadataSourceTests()
        {
            file = new DelegateAssetFile("MyImage.png", "image/png", 1024, () => stream);

            sut = new ImageAssetMetadataSource(assetThumbnailGenerator);
        }

        [Fact]
        public async Task Should_also_enhance_if_type_already_found()
        {
            var command = new CreateAsset { File = file, Type = AssetType.Image };

            await sut.EnhanceAsync(command);

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_tag_if_no_image()
        {
            var command = new CreateAsset { File = file };

            await sut.EnhanceAsync(command);

            Assert.Empty(command.Tags);
        }

        [Fact]
        public async Task Should_get_dimensions_from_image_library()
        {
            var command = new CreateAsset { File = file };

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(new ImageInfo(800, 600, false));

            await sut.EnhanceAsync(command);

            Assert.Equal(800, command.Metadata.GetPixelWidth());
            Assert.Equal(600, command.Metadata.GetPixelHeight());
            Assert.Equal(AssetType.Image, command.Type);

            A.CallTo(() => assetThumbnailGenerator.FixOrientationAsync(stream, A<Stream>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_fix_image_if_oriented()
        {
            var command = new CreateAsset { File = file };

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(new ImageInfo(600, 800, true));

            A.CallTo(() => assetThumbnailGenerator.FixOrientationAsync(stream, A<Stream>._))
                .Returns(new ImageInfo(800, 600, true));

            await sut.EnhanceAsync(command);

            Assert.Equal(800, command.Metadata.GetPixelWidth());
            Assert.Equal(600, command.Metadata.GetPixelHeight());
            Assert.Equal(AssetType.Image, command.Type);

            A.CallTo(() => assetThumbnailGenerator.FixOrientationAsync(stream, A<Stream>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_image_tag_if_small()
        {
            var command = new CreateAsset { File = file, Type = AssetType.Image };

            command.Metadata.SetPixelWidth(100);
            command.Metadata.SetPixelWidth(100);

            await sut.EnhanceAsync(command);

            Assert.Contains("image", command.Tags);
            Assert.Contains("image/small", command.Tags);
        }

        [Fact]
        public async Task Should_add_image_tag_if_medium()
        {
            var command = new CreateAsset { File = file, Type = AssetType.Image };

            command.Metadata.SetPixelWidth(800);
            command.Metadata.SetPixelWidth(600);

            await sut.EnhanceAsync(command);

            Assert.Contains("image", command.Tags);
            Assert.Contains("image/medium", command.Tags);
        }

        [Fact]
        public async Task Should_add_image_tag_if_large()
        {
            var command = new CreateAsset { File = file, Type = AssetType.Image };

            command.Metadata.SetPixelWidth(1200);
            command.Metadata.SetPixelWidth(1400);

            await sut.EnhanceAsync(command);

            Assert.Contains("image", command.Tags);
            Assert.Contains("image/large", command.Tags);
        }

        [Fact]
        public void Should_format_image()
        {
            var source = new AssetEntity
            {
                Metadata = new AssetMetadata
                {
                    ["pixelWidth"] = JsonValue.Create(128),
                    ["pixelHeight"] = JsonValue.Create(55)
                },
                Type = AssetType.Image
            };

            var formatted = sut.Format(source);

            Assert.Equal(new[] { "128x55px" }, formatted);
        }

        [Fact]
        public void Should_not_format_video()
        {
            var source = new AssetEntity { Type = AssetType.Video };

            var formatted = sut.Format(source);

            Assert.Empty(formatted);
        }

        [Fact]
        public void Should_not_format_audio()
        {
            var source = new AssetEntity { Type = AssetType.Audio };

            var formatted = sut.Format(source);

            Assert.Empty(formatted);
        }
    }
}
