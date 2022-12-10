// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable CS0618 // Type or member is obsolete

namespace TestSuite.ApiTests;

[UsesVerify]
public class AssetFormatTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public AssetFormatTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_upload_image_gif_without_extension()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_150kb.gif", "image/gif", Guid.NewGuid().ToString());

        // Should parse image metadata.
        Assert.True(asset.IsImage);
        Assert.Equal(600L, (long)asset.PixelWidth);
        Assert.Equal(600L, asset.Metadata["pixelWidth"]);
        Assert.Equal(400L, (long)asset.PixelHeight);
        Assert.Equal(400L, asset.Metadata["pixelHeight"]);
        Assert.Equal(AssetType.Image, asset.Type);

        await Verify(asset)
            .IgnoreMember<AssetDto>(x => x.FileName)
            .IgnoreMember<AssetDto>(x => x.FileHash)
            .IgnoreMember<AssetDto>(x => x.Slug);
    }

    [Fact]
    public async Task Should_upload_image_gif_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_150kb.gif", "image/gif");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_png_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_400kb.png", "image/png");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_jpg_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_62kb.jpg", "image/jpg");

        await AssertImageAsync(asset);

        Assert.Equal(79L, asset.Metadata["imageQuality"]);
    }

    [Fact]
    public async Task Should_upload_image_webp_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_100kb.webp", "image/jpg");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_tiff_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_400kb.tiff", "image/jpg");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_tga_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_600kb.tga", "image/x-tga");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_bmp_and_resize()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_700kb.bmp", "image/bmp");

        await AssertImageAsync(asset);
    }

    [Fact]
    public async Task Should_upload_image_bmp_and_encode_to_webp()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleImage_700kb.bmp", "image/bmp");

        var (size, type) = await GetReformattedLength(asset.Id, "WEBP");

        Assert.True(size < asset.FileSize);
        Assert.Equal("image/webp", type);

        await Verify(asset);
    }

    private async Task AssertImageAsync(AssetDto asset)
    {
        await Verify(asset);

        // Should parse image metadata.
        Assert.True(asset.IsImage);
        Assert.Equal(600L, (long)asset.PixelWidth);
        Assert.Equal(600L, asset.Metadata["pixelWidth"]);
        Assert.Equal(400L, (long)asset.PixelHeight);
        Assert.Equal(400L, asset.Metadata["pixelHeight"]);
        Assert.Equal(AssetType.Image, asset.Type);

        var resized = await GetResizedLengthAsync(asset.Id, 100, 100);

        Assert.True(resized < asset.FileSize * .25);
    }

    [Fact]
    public async Task Should_fix_orientation()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/logo-wide-rotated.jpg", "image/jpg");

        // Should parse image metadata and fix orientation.
        Assert.True(asset.IsImage);
        Assert.Equal(600L, (long)asset.PixelWidth);
        Assert.Equal(600L, asset.Metadata["pixelWidth"]);
        Assert.Equal(135L, (long)asset.PixelHeight);
        Assert.Equal(135L, asset.Metadata["pixelHeight"]);
        Assert.Equal(79L, asset.Metadata["imageQuality"]);
        Assert.Equal(AssetType.Image, asset.Type);

        await Verify(asset);
    }

    [Fact]
    public async Task Should_upload_audio_mp3()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleAudio_0.4mb.mp3", "audio/mp3");

        // Should parse audio metadata.
        Assert.False(asset.IsImage);
        Assert.Equal("00:00:28.2708750", asset.Metadata["duration"]);
        Assert.Equal(128L, asset.Metadata["audioBitrate"]);
        Assert.Equal(2L, asset.Metadata["audioChannels"]);
        Assert.Equal(44100L, asset.Metadata["audioSampleRate"]);
        Assert.Equal(AssetType.Audio, asset.Type);

        await Verify(asset);
    }

    [Fact]
    public async Task Should_upload_video_mp4()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleVideo_1280x720_1mb.mp4", "audio/mp4");

        // Should parse video metadata.
        Assert.False(asset.IsImage);
        Assert.Equal("00:00:05.3120000", asset.Metadata["duration"]);
        Assert.Equal(384L, asset.Metadata["audioBitrate"]);
        Assert.Equal(2L, asset.Metadata["audioChannels"]);
        Assert.Equal(48000L, asset.Metadata["audioSampleRate"]);
        Assert.Equal(1280L, asset.Metadata["videoWidth"]);
        Assert.Equal(720L, asset.Metadata["videoHeight"]);
        Assert.Equal(AssetType.Video, asset.Type);

        await Verify(asset);
    }

    [Fact]
    public async Task Should_upload_video_mkv()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleVideo_1280x720_1mb.flv", "audio/webm");

        // Should not parse yet.
        Assert.Equal(AssetType.Unknown, asset.Type);

        await Verify(asset);
    }

    [Fact]
    public async Task Should_upload_video_flv()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleVideo_1280x720_1mb.flv", "audio/x-flv");

        // Should not parse yet.
        Assert.Equal(AssetType.Unknown, asset.Type);

        await Verify(asset);
    }

    [Fact]
    public async Task Should_upload_video_3gp()
    {
        var asset = await _.Assets.UploadFileAsync(_.AppName, "Assets/SampleVideo_176x144_1mb.3gp", "audio/3gpp");

        // Should not parse yet.
        Assert.Equal(AssetType.Unknown, asset.Type);

        await Verify(asset);
    }

    private async Task<long> GetResizedLengthAsync(string imageId, int width, int height)
    {
        var url = $"{_.ClientManager.GenerateImageUrl(imageId)}?width={width}&height={height}";

        using (var httpClient = _.ClientManager.CreateHttpClient())
        {
            var response = await httpClient.GetAsync(url);

            await using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var buffer = new MemoryStream();

                await stream.CopyToAsync(buffer);

                return buffer.Length;
            }
        }
    }

    private async Task<(long, string)> GetReformattedLength(string imageId, string format)
    {
        var url = $"{_.ClientManager.GenerateImageUrl(imageId)}?format={format}";

        using (var httpClient = _.ClientManager.CreateHttpClient())
        {
            var response = await httpClient.GetAsync(url);

            await using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var buffer = new MemoryStream();

                await stream.CopyToAsync(buffer);

                return (buffer.Length, response.Content.Headers.ContentType.ToString());
            }
        }
    }
}
