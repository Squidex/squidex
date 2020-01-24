// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class ImageAssetMetadataSourceTests
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly HashSet<string> tags = new HashSet<string>();
        private readonly MemoryStream stream = new MemoryStream();
        private readonly AssetFile file;
        private readonly ImageAssetMetadataSource sut;

        public ImageAssetMetadataSourceTests()
        {
            file = new AssetFile("MyImage.png", "image/png", 1024, () => stream);

            sut = new ImageAssetMetadataSource(assetThumbnailGenerator);
        }

        [Fact]
        public async Task Should_not_enhance_if_type_already_found()
        {
            var command = new CreateAsset { File = file, Type = AssetType.Image };

            await sut.EnhanceAsync(command, tags);

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(A<Stream>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_tag_if_no_image()
        {
            var command = new CreateAsset { File = file };

            await sut.EnhanceAsync(command, tags);

            Assert.Empty(tags);
        }

        [Fact]
        public async Task Should_add_image_tag_if_small()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(new ImageInfo(100, 100));

            var command = new CreateAsset { File = file };

            await sut.EnhanceAsync(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/small", tags);
        }

        [Fact]
        public async Task Should_add_image_tag_if_medium()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(new ImageInfo(800, 600));

            var command = new CreateAsset { File = file };

            await sut.EnhanceAsync(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/medium", tags);
        }

        [Fact]
        public async Task Should_add_image_tag_if_large()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(new ImageInfo(1200, 1400));

            var command = new CreateAsset { File = file };

            await sut.EnhanceAsync(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/large", tags);
        }

        [Fact]
        public void Should_format_to_empty()
        {
            var source = new AssetEntity();

            var formatted = sut.Format(source);

            Assert.Empty(formatted);
        }
    }
}
