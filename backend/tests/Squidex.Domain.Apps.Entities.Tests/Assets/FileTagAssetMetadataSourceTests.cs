// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Assets;

public class FileTagAssetMetadataSourceTests
{
    private readonly FileTagAssetMetadataSource sut = new FileTagAssetMetadataSource();

    [Fact]
    public async Task Should_ignore_files_without_extension()
    {
        var command = FakeCommand("NoExtension");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Unknown, command.Type);
    }

    [Fact]
    public async Task Should_provide_metadata_for_image()
    {
        var command = Command("SamplePNGImage_100kbmb.png");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Image, command.Type);

        Assert.Equal(272, command.Metadata.GetPixelWidth());
        Assert.Equal(170, command.Metadata.GetPixelHeight());
    }

    [Fact]
    public async Task Should_not_set_image_height_and_width_metadata_when_file_does_not_have_those_values()
    {
        var command = Command("SampleAudio_0.4mb.mp3");

        await sut.EnhanceAsync(command, default);

        Assert.Null(command.Metadata.GetPixelWidth());
        Assert.Null(command.Metadata.GetPixelHeight());
    }

    [Fact]
    public async Task Should_provide_metadata_for_audio()
    {
        var command = Command("SampleAudio_0.4mb.mp3");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Audio, command.Type);

        Assert.Equal(JsonValue.Create("00:00:28.2708750"), command.Metadata["duration"]);
        Assert.Equal(JsonValue.Create(128L), command.Metadata["audioBitrate"]);
        Assert.Equal(JsonValue.Create(2L), command.Metadata["audioChannels"]);
        Assert.Equal(JsonValue.Create(44100L), command.Metadata["audioSampleRate"]);
    }

    [Fact]
    public async Task Should_provide_metadata_for_video()
    {
        var command = Command("SampleVideo_1280x720_1mb.mp4");

        await sut.EnhanceAsync(command, default);

        Assert.Equal(AssetType.Video, command.Type);

        Assert.Equal(JsonValue.Create("00:00:05.3120000"), command.Metadata["duration"]);
        Assert.Equal(JsonValue.Create(384L), command.Metadata["audioBitrate"]);
        Assert.Equal(JsonValue.Create(2L), command.Metadata["audioChannels"]);
        Assert.Equal(JsonValue.Create(48000L), command.Metadata["audioSampleRate"]);
        Assert.Equal(JsonValue.Create(1280L), command.Metadata["videoWidth"]);
        Assert.Equal(JsonValue.Create(720L), command.Metadata["videoHeight"]);
    }

    [Fact]
    public void Should_format_video()
    {
        var source = new AssetEntity
        {
            Metadata = new AssetMetadata
            {
                ["videoWidth"] = JsonValue.Create(128),
                ["videoHeight"] = JsonValue.Create(55),
                ["duration"] = JsonValue.Create("00:10:12")
            },
            Type = AssetType.Video
        };

        var formatted = sut.Format(source);

        Assert.Equal(new[] { "128x55pt", "00:10:12" }, formatted);
    }

    [Fact]
    public void Should_format_audio()
    {
        var source = new AssetEntity
        {
            Metadata = new AssetMetadata
            {
                ["duration"] = JsonValue.Create("00:10:12")
            },
            Type = AssetType.Audio
        };

        var formatted = sut.Format(source);

        Assert.Equal(new[] { "00:10:12" }, formatted.ToArray());
    }

    [Fact]
    public void Should_not_format_image()
    {
        var source = new AssetEntity { Type = AssetType.Image };

        var formatted = sut.Format(source);

        Assert.Empty(formatted);
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
