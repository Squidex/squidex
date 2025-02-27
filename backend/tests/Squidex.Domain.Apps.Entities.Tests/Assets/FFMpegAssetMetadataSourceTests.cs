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
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Assets;

public class FFMpegAssetMetadataSourceTests : GivenContext
{
    private readonly FFMpegAssetMetadataSource sut = new FFMpegAssetMetadataSource();

    [Fact]
    public async Task Should_ignore_files_with_type()
    {
        var command = FakeCommand(AssetType.Image);

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Image, command.Type);
    }

    [Fact]
    public async Task Should_not_set_image_height_and_width_metadata_when_file_does_not_have_those_values()
    {
        var command = Command("SampleAudio_0.4mb.mp3");

        await sut.EnhanceAsync(command, default);

        Assert.Null(command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Null(command.Metadata.GetInt32(KnownMetadataKeys.PixelHeight));
    }

    [Fact]
    public async Task Should_provide_metadata_for_audio()
    {
        var command = Command("SampleAudio_0.4mb.mp3");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Audio, command.Type);
        Assert.Equal(JsonValue.Create(128L), command.Metadata[KnownMetadataKeys.AudioBitrate]);
        Assert.Equal(JsonValue.Create(44100L), command.Metadata[KnownMetadataKeys.AudioSampleRate]);
    }

    [Fact]
    public async Task Should_provide_metadata_for_broken_video()
    {
        var command = Command("SampleVideo_Broken.mp4");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Video, command.Type);
        Assert.Equal(JsonValue.Create("00:00:11"), command.Metadata[KnownMetadataKeys.Duration]);
        Assert.Equal(JsonValue.Create(317L), command.Metadata[KnownMetadataKeys.AudioBitrate]);
        Assert.Equal(JsonValue.Create(48000L), command.Metadata[KnownMetadataKeys.AudioSampleRate]);
        Assert.Equal(JsonValue.Create(1080L), command.Metadata[KnownMetadataKeys.VideoHeight]);
        Assert.Equal(JsonValue.Create(1920L), command.Metadata[KnownMetadataKeys.VideoWidth]);
    }

    [Fact]
    public void Should_not_format_asset()
    {
        var source = CreateAsset() with
        {
            Type = AssetType.Image,
        };

        var formatted = sut.Format(source);

        Assert.Empty(formatted);
    }

    private static UploadAssetCommand Command(string path)
    {
        var file = new FileInfo(Path.Combine("Assets", "TestFiles", path));

        return new CreateAsset
        {
            File = new DelegateAssetFile(file.Name, "mime", file.Length, file.OpenRead),
        };
    }

    private static UploadAssetCommand FakeCommand(AssetType type)
    {
        return new CreateAsset
        {
            Type = type,
        };
    }
}
