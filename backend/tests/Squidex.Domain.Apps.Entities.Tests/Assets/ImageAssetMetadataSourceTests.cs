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

namespace Squidex.Domain.Apps.Entities.Assets;

public class ImageAssetMetadataSourceTests : GivenContext
{
    private readonly IAssetThumbnailGenerator assetGenerator = A.Fake<IAssetThumbnailGenerator>();
    private readonly IAssetFile file;
    private readonly MemoryStream stream = new MemoryStream();
    private readonly ImageAssetMetadataSource sut;

    public ImageAssetMetadataSourceTests()
    {
        file = new DelegateAssetFile("MyImage.png", "image/png", 1024, () => stream);

        sut = new ImageAssetMetadataSource(assetGenerator);
    }

    [Fact]
    public async Task Should_also_enhance_if_type_already_found()
    {
        var command = new CreateAsset { File = file, Type = AssetType.Image };

        await sut.EnhanceAsync(command, CancellationToken);

        A.CallTo(() => assetGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_add_tag_if_no_image()
    {
        var command = new CreateAsset { File = file };

        A.CallTo(() => assetGenerator.GetImageInfoAsync(stream, file.MimeType, CancellationToken))
            .Returns(Task.FromResult<ImageInfo?>(null));

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Empty(command.Tags);
    }

    [Fact]
    public async Task Should_get_dimensions_from_image_library()
    {
        var command = new CreateAsset { File = file };

        A.CallTo(() => assetGenerator.GetImageInfoAsync(stream, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 800, 600, ImageOrientation.None, false));

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Equal(800, command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Equal(600, command.Metadata.GetInt32(KnownMetadataKeys.PixelHeight));
        Assert.Equal(AssetType.Image, command.Type);

        A.CallTo(() => assetGenerator.FixAsync(stream, file.MimeType, A<Stream>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_fix_image_if_oriented()
    {
        var command = new CreateAsset { File = file };

        A.CallTo(() => assetGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 800, 600, ImageOrientation.None, false));

        A.CallTo(() => assetGenerator.GetImageInfoAsync(stream, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 800, 600, ImageOrientation.BottomRight, false)).Once();

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Equal(800, command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Equal(600, command.Metadata.GetInt32(KnownMetadataKeys.PixelHeight));
        Assert.Equal(AssetType.Image, command.Type);

        A.CallTo(() => assetGenerator.FixAsync(stream, file.MimeType, A<Stream>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_fix_image_if_it_contains_sensitive_metadata()
    {
        var command = new CreateAsset { File = file };

        A.CallTo(() => assetGenerator.GetImageInfoAsync(A<Stream>._, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 800, 600, ImageOrientation.None, false));

        A.CallTo(() => assetGenerator.GetImageInfoAsync(stream, file.MimeType, CancellationToken))
            .Returns(new ImageInfo(ImageFormat.PNG, 800, 600, ImageOrientation.None, true)).Once();

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Equal(800, command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Equal(600, command.Metadata.GetInt32(KnownMetadataKeys.PixelHeight));
        Assert.Equal(AssetType.Image, command.Type);

        A.CallTo(() => assetGenerator.FixAsync(stream, file.MimeType, A<Stream>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_add_image_tag_if_small()
    {
        var command = new CreateAsset { File = file, Type = AssetType.Image };

        command.Metadata[KnownMetadataKeys.PixelWidth] = 100;
        command.Metadata[KnownMetadataKeys.PixelHeight] = 50;

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Contains("image", command.Tags);
        Assert.Contains("image/small", command.Tags);
    }

    [Fact]
    public async Task Should_add_image_tag_if_medium()
    {
        var command = new CreateAsset { File = file, Type = AssetType.Image };

        command.Metadata[KnownMetadataKeys.PixelWidth] = 800;
        command.Metadata[KnownMetadataKeys.PixelHeight] = 600;

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Contains("image", command.Tags);
        Assert.Contains("image/medium", command.Tags);
    }

    [Fact]
    public async Task Should_add_image_tag_if_large()
    {
        var command = new CreateAsset { File = file, Type = AssetType.Image };

        command.Metadata[KnownMetadataKeys.PixelWidth] = 1400;
        command.Metadata[KnownMetadataKeys.PixelHeight] = 1200;

        await sut.EnhanceAsync(command, CancellationToken);

        Assert.Contains("image", command.Tags);
        Assert.Contains("image/large", command.Tags);
    }

    [Fact]
    public void Should_format_image()
    {
        var source = CreateAsset() with
        {
            Metadata = new AssetMetadata
            {
                [KnownMetadataKeys.PixelWidth] = 128,
                [KnownMetadataKeys.PixelHeight] = 55
            },
            Type = AssetType.Image
        };

        var formatted = sut.Format(source);

        Assert.Equal(new[] { "128x55px" }, formatted);
    }

    [Fact]
    public void Should_not_format_video()
    {
        var source = CreateAsset() with
        {
            Type = AssetType.Video
        };

        var formatted = sut.Format(source);

        Assert.Empty(formatted);
    }

    [Fact]
    public void Should_not_format_audio()
    {
        var source = CreateAsset() with
        {
            Type = AssetType.Audio
        };

        var formatted = sut.Format(source);

        Assert.Empty(formatted);
    }
}
