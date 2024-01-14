// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class AssetsValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = [];
    private static readonly Asset Document = TestAssets.Document(DomainId.NewGuid());
    private static readonly Asset Image1 = TestAssets.Image(DomainId.NewGuid());
    private static readonly Asset Image2 = TestAssets.Image(DomainId.NewGuid());
    private static readonly Asset ImageSvg = TestAssets.Svg(DomainId.NewGuid());
    private static readonly Asset Video = TestAssets.Video(DomainId.NewGuid());

    public static readonly TheoryData<DomainId> AssetsWithDimensions = new TheoryData<DomainId>
    {
        { Image1.Id },
        { Video.Id }
    };

    [Fact]
    public async Task Should_not_add_error_if_assets_are_valid()
    {
        var sut = Validator(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(Document.Id), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_assets_are_null_but_not_required()
    {
        var sut = Validator(new AssetsFieldProperties());

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_assets_are_empty_but_not_required()
    {
        var sut = Validator(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_duplicates_are_allowed()
    {
        var sut = Validator(new AssetsFieldProperties { AllowDuplicates = true });

        await sut.ValidateAsync(CreateValue(Image1.Id, Image1.Id), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_asset_is_an_image()
    {
        var sut = Validator(new AssetsFieldProperties { ExpectedType = AssetType.Image });

        await sut.ValidateAsync(CreateValue(ImageSvg.Id, Image1.Id), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_references_are_required()
    {
        var sut = Validator(new AssetsFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_asset_are_not_valid()
    {
        var id = DomainId.NewGuid();

        var sut = Validator(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(id), errors);

        errors.Should().BeEquivalentTo(
            new[] { $"[1]: Id {id} not found." });
    }

    [Fact]
    public async Task Should_add_error_if_document_is_too_small()
    {
        var sut = Validator(new AssetsFieldProperties { MinSize = 5 * 1024 });

        await sut.ValidateAsync(CreateValue(Document.Id, Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[1]: Size of 4 kB must be greater than 5 kB." });
    }

    [Fact]
    public async Task Should_add_error_if_document_is_too_big()
    {
        var sut = Validator(new AssetsFieldProperties { MaxSize = 5 * 1024 });

        await sut.ValidateAsync(CreateValue(Document.Id, Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Size of 8 kB must be less than 5 kB." });
    }

    [Fact]
    public async Task Should_add_error_if_document_is_not_an_image()
    {
        var sut = Validator(new AssetsFieldProperties { ExpectedType = AssetType.Image });

        await sut.ValidateAsync(CreateValue(Document.Id, Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[1]: Not of expected type: Image." });
    }

    [Theory]
    [MemberData(nameof(AssetsWithDimensions))]
    public async Task Should_add_error_if_asset_width_is_too_small(DomainId videoOrImageId)
    {
        var sut = Validator(new AssetsFieldProperties { MinWidth = 1000 });

        await sut.ValidateAsync(CreateValue(Document.Id, videoOrImageId), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Width 800px must be greater than 1000px." });
    }

    [Theory]
    [MemberData(nameof(AssetsWithDimensions))]
    public async Task Should_add_error_if_asset_width_is_too_big(DomainId videoOrImageId)
    {
        var sut = Validator(new AssetsFieldProperties { MaxWidth = 700 });

        await sut.ValidateAsync(CreateValue(Document.Id, videoOrImageId), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Width 800px must be less than 700px." });
    }

    [Theory]
    [MemberData(nameof(AssetsWithDimensions))]
    public async Task Should_add_error_if_asset_height_is_too_small(DomainId videoOrImageId)
    {
        var sut = Validator(new AssetsFieldProperties { MinHeight = 800 });

        await sut.ValidateAsync(CreateValue(Document.Id, videoOrImageId), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Height 600px must be greater than 800px." });
    }

    [Theory]
    [MemberData(nameof(AssetsWithDimensions))]
    public async Task Should_add_error_if_asset_height_is_too_big(DomainId videoOrImageId)
    {
        var sut = Validator(new AssetsFieldProperties { MaxHeight = 500 });

        await sut.ValidateAsync(CreateValue(Document.Id, videoOrImageId), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Height 600px must be less than 500px." });
    }

    [Theory]
    [MemberData(nameof(AssetsWithDimensions))]
    public async Task Should_add_error_if_asset_has_invalid_aspect_ratio(DomainId videoOrImageId)
    {
        var sut = Validator(new AssetsFieldProperties { AspectWidth = 1, AspectHeight = 1 });

        await sut.ValidateAsync(CreateValue(Document.Id, videoOrImageId), errors);

        errors.Should().BeEquivalentTo(
            new[] { "[2]: Must have aspect ratio 1:1." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_enough_items()
    {
        var sut = Validator(new AssetsFieldProperties { MinItems = 2 });

        await sut.ValidateAsync(CreateValue(Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 2 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_too_much_items()
    {
        var sut = Validator(new AssetsFieldProperties { MaxItems = 1 });

        await sut.ValidateAsync(CreateValue(Image1.Id, Image2.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 1 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_reference_contains_duplicate_values()
    {
        var sut = Validator(new AssetsFieldProperties());

        await sut.ValidateAsync(CreateValue(Image1.Id, Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not contain duplicate values." });
    }

    [Fact]
    public async Task Should_add_error_if_image_has_invalid_extension()
    {
        var sut = Validator(new AssetsFieldProperties { AllowedExtensions = ReadonlyList.Create("mp4") });

        await sut.ValidateAsync(CreateValue(Document.Id, Image1.Id), errors);

        errors.Should().BeEquivalentTo(
            new[]
            {
                "[1]: Must be an allowed extension.",
                "[2]: Must be an allowed extension."
            });
    }

    private static object CreateValue(params DomainId[] ids)
    {
        return ids.ToList();
    }

    private static IValidator Validator(AssetsFieldProperties properties)
    {
        return new AssetsValidator(properties.IsRequired, properties, FoundAssets());
    }

    private static CheckAssets FoundAssets()
    {
        return (ids, ct) =>
        {
            var actual = new List<Asset> { Document, Image1, Image2, ImageSvg, Video };

            return Task.FromResult<IReadOnlyList<Asset>>(actual);
        };
    }
}
