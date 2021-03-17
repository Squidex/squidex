// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable CS0618 // Type or member is obsolete

namespace TestSuite.ApiTests
{
    public class AssetFormatTests : IClassFixture<AssetFixture>
    {
        public AssetFixture _ { get; }

        public AssetFormatTests(AssetFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_upload_image_gif()
        {
            var asset = await _.UploadFileAsync("Assets/SampleGIFImage_40kbmb.gif", "image/gif");

            // Should parse image metadata.
            Assert.True(asset.IsImage);
            Assert.Equal(312, asset.PixelHeight);
            Assert.Equal(312, asset.PixelWidth);
            Assert.Equal(312L, asset.Metadata["pixelHeight"]);
            Assert.Equal(312L, asset.Metadata["pixelWidth"]);
            Assert.Equal(AssetType.Image, asset.Type);
        }

        [Fact]
        public async Task Should_upload_image_gif_without_extension()
        {
            var asset = await _.UploadFileAsync("Assets/SampleGIFImage_40kbmb.gif", "image/gif", Guid.NewGuid().ToString());

            // Should parse image metadata.
            Assert.True(asset.IsImage);
            Assert.Equal(312, asset.PixelHeight);
            Assert.Equal(312, asset.PixelWidth);
            Assert.Equal(312L, asset.Metadata["pixelHeight"]);
            Assert.Equal(312L, asset.Metadata["pixelWidth"]);
            Assert.Equal(AssetType.Image, asset.Type);
        }

        [Fact]
        public async Task Should_upload_image_png()
        {
            var asset = await _.UploadFileAsync("Assets/SamplePNGImage_100kbmb.png", "image/png");

            // Should parse image metadata.
            Assert.True(asset.IsImage);
            Assert.Equal(170, asset.PixelHeight);
            Assert.Equal(272, asset.PixelWidth);
            Assert.Equal(170L, asset.Metadata["pixelHeight"]);
            Assert.Equal(272L, asset.Metadata["pixelWidth"]);
            Assert.Equal(AssetType.Image, asset.Type);
        }

        [Fact]
        public async Task Should_upload_image_jpg()
        {
            var asset = await _.UploadFileAsync("Assets/SampleJPGImage_50kbmb.jpg", "image/jpg");

            // Should parse image metadata.
            Assert.True(asset.IsImage);
            Assert.Equal(300, asset.PixelHeight);
            Assert.Equal(300, asset.PixelWidth);
            Assert.Equal(300L, asset.Metadata["pixelHeight"]);
            Assert.Equal(300L, asset.Metadata["pixelWidth"]);
            Assert.Equal(96L, asset.Metadata["imageQuality"]);
            Assert.Equal(AssetType.Image, asset.Type);
        }

        [Fact]
        public async Task Should_fix_orientation()
        {
            var asset = await _.UploadFileAsync("Assets/logo-wide-rotated.jpg", "image/jpg");

            // Should parse image metadata and fix orientation.
            Assert.True(asset.IsImage);
            Assert.Equal(135, asset.PixelHeight);
            Assert.Equal(600, asset.PixelWidth);
            Assert.Equal(135L, asset.Metadata["pixelHeight"]);
            Assert.Equal(600L, asset.Metadata["pixelWidth"]);
            Assert.Equal(79L, asset.Metadata["imageQuality"]);
            Assert.Equal(AssetType.Image, asset.Type);
        }

        [Fact]
        public async Task Should_upload_audio_mp3()
        {
            var asset = await _.UploadFileAsync("Assets/SampleAudio_0.4mb.mp3", "audio/mp3");

            // Should parse audio metadata.
            Assert.False(asset.IsImage);
            Assert.Equal("00:00:28.2708750", asset.Metadata["duration"]);
            Assert.Equal(128L, asset.Metadata["audioBitrate"]);
            Assert.Equal(2L, asset.Metadata["audioChannels"]);
            Assert.Equal(44100L, asset.Metadata["audioSampleRate"]);
            Assert.Equal(AssetType.Audio, asset.Type);
        }

        [Fact]
        public async Task Should_upload_video_mp4()
        {
            var asset = await _.UploadFileAsync("Assets/SampleVideo_1280x720_1mb.mp4", "audio/mp4");

            // Should parse video metadata.
            Assert.False(asset.IsImage);
            Assert.Equal("00:00:05.3120000", asset.Metadata["duration"]);
            Assert.Equal(384L, asset.Metadata["audioBitrate"]);
            Assert.Equal(2L, asset.Metadata["audioChannels"]);
            Assert.Equal(48000L, asset.Metadata["audioSampleRate"]);
            Assert.Equal(1280L, asset.Metadata["videoWidth"]);
            Assert.Equal(720L, asset.Metadata["videoHeight"]);
            Assert.Equal(AssetType.Video, asset.Type);
        }

        [Fact]
        public async Task Should_upload_video_mkv()
        {
            var asset = await _.UploadFileAsync("Assets/SampleVideo_1280x720_1mb.flv", "audio/webm");

            // Should not parse yet.
            Assert.Equal(AssetType.Unknown, asset.Type);
        }

        [Fact]
        public async Task Should_upload_video_flv()
        {
            var asset = await _.UploadFileAsync("Assets/SampleVideo_1280x720_1mb.flv", "audio/x-flv");

            // Should not parse yet.
            Assert.Equal(AssetType.Unknown, asset.Type);
        }

        [Fact]
        public async Task Should_upload_video_3gp()
        {
            var asset = await _.UploadFileAsync("Assets/SampleVideo_176x144_1mb.3gp", "audio/3gpp");

            // Should not parse yet.
            Assert.Equal(AssetType.Unknown, asset.Type);
        }
    }
}
