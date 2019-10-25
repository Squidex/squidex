// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class ImageTagGeneratorTests
    {
        private readonly HashSet<string> tags = new HashSet<string>();
        private readonly ImageTagGenerator sut = new ImageTagGenerator();

        [Fact]
        public void Should_not_add_tag_if_no_image()
        {
            var command = new CreateAsset();

            sut.GenerateTags(command, tags);

            Assert.Empty(tags);
        }

        [Fact]
        public void Should_add_image_tag_if_small()
        {
            var command = new CreateAsset
            {
                ImageInfo = new ImageInfo(100, 100)
            };

            sut.GenerateTags(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/small", tags);
        }

        [Fact]
        public void Should_add_image_tag_if_medium()
        {
            var command = new CreateAsset
            {
                ImageInfo = new ImageInfo(800, 600)
            };

            sut.GenerateTags(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/medium", tags);
        }

        [Fact]
        public void Should_add_image_tag_if_large()
        {
            var command = new CreateAsset
            {
                ImageInfo = new ImageInfo(1200, 1400)
            };

            sut.GenerateTags(command, tags);

            Assert.Contains("image", tags);
            Assert.Contains("image/large", tags);
        }
    }
}
