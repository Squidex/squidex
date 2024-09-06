// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets;

public class SvgAssetMetadataSourceTests : GivenContext
{
    private readonly SvgAssetMetadataSource sut = new SvgAssetMetadataSource();

    public SvgAssetMetadataSourceTests()
    {
        sut = new SvgAssetMetadataSource();
    }

    [Fact]
    public async Task Should_add_image_tag_if_svg_mime()
    {
        var command = FakeCommand("image.svg");

        await sut.EnhanceAsync(command, default);

        Assert.Contains("image", command.Tags);
    }

    [Fact]
    public async Task Should_add_image_tag_if_svg_extension()
    {
        var command = FakeCommand("image.svg");

        await sut.EnhanceAsync(command, default);

        Assert.Contains("image", command.Tags);
    }

    [Fact]
    public async Task Should_throw_exception_if_svg_is_malicious()
    {
        var command = Command("SvgInvalid.svg");

        await Assert.ThrowsAsync<ValidationException>(() => sut.EnhanceAsync(command, default));

        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.PixelWidth));
        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.PixelHeight));
        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.SvgWidth));
        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.SvgHeight));
        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.SvgViewBox));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_svg_is_not_malicious()
    {
        var command = Command("SvgValid.svg");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(20, command.Metadata[KnownMetadataKeys.PixelWidth].AsNumber);
        Assert.Equal(30, command.Metadata[KnownMetadataKeys.PixelHeight].AsNumber);

        Assert.Equal(20, command.Metadata[KnownMetadataKeys.SvgWidth].AsNumber);
        Assert.Equal(30, command.Metadata[KnownMetadataKeys.SvgHeight].AsNumber);

        Assert.Equal("0 0 100 100", command.Metadata[KnownMetadataKeys.SvgViewBox].AsString);
    }

    [Fact]
    public async Task Should_annote_with_non_numeric_dimensions()
    {
        var command = Command("SvgValidNonNumberSizes.svg");

        await sut.EnhanceAsync(command, default);

        Assert.DoesNotContain(KnownMetadataKeys.PixelWidth, command.Metadata);
        Assert.DoesNotContain(KnownMetadataKeys.PixelHeight, command.Metadata);

        Assert.Equal("20%", command.Metadata[KnownMetadataKeys.SvgWidth].AsString);
        Assert.Equal("30%", command.Metadata[KnownMetadataKeys.SvgHeight].AsString);

        Assert.Equal("0 0 100 100", command.Metadata[KnownMetadataKeys.SvgViewBox].AsString);
    }

    [Fact]
    public async Task Should_extract_metadata_if_not_pixel_dimensions()
    {
        var command = Command("SvgNonPixelDimensions.svg");

        await sut.EnhanceAsync(command, default);

        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.PixelWidth));
        Assert.False(command.Metadata.ContainsKey(KnownMetadataKeys.PixelHeight));

        Assert.Equal("2rem", command.Metadata[KnownMetadataKeys.SvgWidth].AsString);
        Assert.Equal("3rem", command.Metadata[KnownMetadataKeys.SvgHeight].AsString);

        Assert.Equal("0 0 100 100", command.Metadata[KnownMetadataKeys.SvgViewBox].AsString);
    }

    [Fact]
    public void Should_describe_metadata()
    {
        var source = CreateAsset() with
        {
            Metadata = new AssetMetadata
            {
                [KnownMetadataKeys.SvgWidth] = "128",
                [KnownMetadataKeys.SvgHeight] = "55"
            },
            MimeType = "image/svg+xml"
        };

        var formatted = sut.Format(source);

        Assert.Equal(["128x55"], formatted);
    }

    [Fact]
    public void Should_describe_metadata_with_number()
    {
        var source = CreateAsset() with
        {
            Metadata = new AssetMetadata
            {
                [KnownMetadataKeys.SvgWidth] = 128,
                [KnownMetadataKeys.SvgHeight] = 55
            },
            MimeType = "image/svg+xml"
        };

        var formatted = sut.Format(source);

        Assert.Equal(["128x55"], formatted);
    }

    private static UploadAssetCommand Command(string path)
    {
        var file = new FileInfo(Path.Combine("Assets", "TestFiles", path));

        return new CreateAsset
        {
            File = new DelegateAssetFile(file.Name, "mime", file.Length, file.OpenRead)
        };
    }

    private static UploadAssetCommand FakeCommand(string name)
    {
        var stream = new MemoryStream();

        return new CreateAsset
        {
            File = new DelegateAssetFile(name, "mime", stream.Length, () => stream)
        };
    }
}
