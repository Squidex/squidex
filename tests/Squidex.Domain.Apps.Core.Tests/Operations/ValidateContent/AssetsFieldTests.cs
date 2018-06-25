// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class AssetsFieldTests
    {
        private readonly List<string> errors = new List<string>();

        public sealed class AssetInfo : IAssetInfo
        {
            public Guid AssetId { get; set; }

            public string FileName { get; set; }

            public long FileSize { get; set; }

            public bool IsImage { get; set; }

            public int? PixelWidth { get; set; }

            public int? PixelHeight { get; set; }
        }

        private readonly AssetInfo document = new AssetInfo
        {
            AssetId = Guid.NewGuid(),
            FileName = "MyDocument.pdf",
            FileSize = 1024 * 4,
            IsImage = false,
            PixelWidth = null,
            PixelHeight = null
        };

        private readonly AssetInfo image = new AssetInfo
        {
            AssetId = Guid.NewGuid(),
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            IsImage = true,
            PixelWidth = 800,
            PixelHeight = 600
        };

        private readonly ValidationContext ctx;

        public AssetsFieldTests()
        {
            ctx = ValidationTestExtensions.Assets(image, document);
        }

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new AssetsFieldProperties());

            Assert.Equal("my-assets", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_valid()
        {
            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync(CreateValue(document.AssetId), errors, ctx);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_null_and_valid()
        {
            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors, ctx);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_null()
        {
            var sut = Field(new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_assets_are_required_and_empty()
        {
            var sut = Field(new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync("invalid", errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_not_enough_items()
        {
            var sut = Field(new AssetsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(document.AssetId, document.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_too_much_items()
        {
            var sut = Field(new AssetsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(document.AssetId, document.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { "Must have not more than 1 item(s)." });
        }

        [Fact]
        public async Task Should_add_errors_if_asset_are_not_valid()
        {
            var assetId = Guid.NewGuid();

            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync(CreateValue(assetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[1]: Id '{assetId}' not found." });
        }

        [Fact]
        public async Task Should_add_error_if_document_is_too_small()
        {
            var sut = Field(new AssetsFieldProperties { MinSize = 5 * 1024 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[1]: '4 kB' less than minimum of '5 kB'." });
        }

        [Fact]
        public async Task Should_add_error_if_document_is_too_big()
        {
            var sut = Field(new AssetsFieldProperties { MaxSize = 5 * 1024 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[2]: '8 kB' greater than maximum of '5 kB'." });
        }

        [Fact]
        public async Task Should_add_error_if_document_is_not_an_image()
        {
            var sut = Field(new AssetsFieldProperties { MustBeImage = true });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[1]: Not an image." });
        }

        [Fact]
        public async Task Should_add_error_if_image_width_is_too_small()
        {
            var sut = Field(new AssetsFieldProperties { MinWidth = 1000 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[2]: Width '800px' less than minimum of '1000px'." });
        }

        [Fact]
        public async Task Should_add_error_if_image_width_is_too_big()
        {
            var sut = Field(new AssetsFieldProperties { MaxWidth = 700 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[2]: Width '800px' greater than maximum of '700px'." });
        }

        [Fact]
        public async Task Should_add_error_if_image_height_is_too_small()
        {
            var sut = Field(new AssetsFieldProperties { MinHeight = 800 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[2]: Height '600px' less than minimum of '800px'." });
        }

        [Fact]
        public async Task Should_add_error_if_image_height_is_too_big()
        {
            var sut = Field(new AssetsFieldProperties { MaxHeight = 500 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { $"[2]: Height '600px' greater than maximum of '500px'." });
        }

        [Fact]
        public async Task Should_add_error_if_image_has_invalid_aspect_ratio()
        {
            var sut = Field(new AssetsFieldProperties { AspectWidth = 1, AspectHeight = 1 });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[] { "[2]: Aspect ratio not '1:1'." });
        }

        [Fact]
        public async Task Should_add_error_if_image_has_invalid_extension()
        {
            var sut = Field(new AssetsFieldProperties { AllowedExtensions = ImmutableList.Create("mp4") });

            await sut.ValidateAsync(CreateValue(document.AssetId, image.AssetId), errors, ctx);

            errors.Should().BeEquivalentTo(
                new[]
                {
                    $"[1]: Invalid file extension.",
                    $"[2]: Invalid file extension."
                });
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids.OfType<object>().ToArray());
        }

        private static RootField<AssetsFieldProperties> Field(AssetsFieldProperties properties)
        {
            return Fields.Assets(1, "my-assets", Partitioning.Invariant, properties);
        }
    }
}
