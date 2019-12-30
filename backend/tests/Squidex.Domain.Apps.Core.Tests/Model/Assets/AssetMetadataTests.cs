// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Assets
{
    public class AssetMetadataTests
    {
        [Fact]
        public void Should_return_pixel_infos_if_found()
        {
            var sut =
                new AssetMetadata()
                    .SetPixelWidth(800)
                    .SetPixelHeight(600);

            Assert.Equal(800, sut.GetPixelWidth());
            Assert.Equal(600, sut.GetPixelHeight());
        }

        [Fact]
        public void Should_return_null_if_pixel_infos_not_found()
        {
            var sut = new AssetMetadata();

            Assert.Null(sut.GetPixelWidth());
            Assert.Null(sut.GetPixelHeight());
        }

        [Fact]
        public void Should_return_self_if_path_is_empty()
        {
            var sut = new AssetMetadata();

            var found = sut.TryGetByPath(string.Empty, out var result);

            Assert.False(found);
            Assert.Same(sut, result);
        }

        [Fact]
        public void Should_return_plain_value_if_found()
        {
            var sut = new AssetMetadata().SetPixelWidth(800);

            var found = sut.TryGetByPath("pixelWidth", out var result);

            Assert.True(found);
            Assert.Equal(JsonValue.Create(800), result);
        }

        [Fact]
        public void Should_return_null_if_not_found()
        {
            var sut = new AssetMetadata().SetPixelWidth(800);

            var found = sut.TryGetByPath("pixelHeight", out var result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void Should_return_nested_value_if_found()
        {
            var sut = new AssetMetadata
            {
                ["meta"] =
                    JsonValue.Object()
                        .Add("nested1",
                            JsonValue.Object()
                                .Add("nested2", 12))
            };

            var found = sut.TryGetByPath("meta.nested1.nested2", out var result);

            Assert.True(found);
            Assert.Equal(JsonValue.Create(12), result);
        }
    }
}
