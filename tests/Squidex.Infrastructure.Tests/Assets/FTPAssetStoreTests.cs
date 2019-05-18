// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentFTP;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class FTPAssetStoreTests : IClassFixture<FTPAssetStoreFixture>
    {
        private readonly FTPAssetStoreFixture fixture;

        private readonly string ftpMessage = "The system cannot find the file specified";

        private readonly string source = "/file1";
        private readonly string target = "/file2";
        private readonly string cannotfindfile = "/cannotfindfile";

        public FTPAssetStoreTests(FTPAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task InitializeAsyncTest()
        {
            var token = A.Dummy<CancellationToken>();
            await fixture.AssetStore.InitializeAsync(token);

            A.CallTo(() => fixture.FtpClient.ConnectAsync(token)).MustHaveHappened();
            A.CallTo(() => fixture.FtpClient.DirectoryExistsAsync("/", token)).Returns(false);
            A.CallTo(() => fixture.FtpClient.ConnectAsync(token)).MustHaveHappened();
            A.CallTo(() => fixture.FtpClient.CreateDirectoryAsync("/", token)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_copy_files()
        {
            var token = A.Dummy<CancellationToken>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(null, target, token));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(source, null, token));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(null, null, token));

            A.CallTo(() => fixture.FtpClient.UploadAsync(A<Stream>._, cannotfindfile, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, token))
                .ThrowsAsync(new FtpException(ftpMessage, new Exception(ftpMessage)));

            await Assert.ThrowsAsync<AssetNotFoundException>(async () => await fixture.AssetStore.CopyAsync(source, cannotfindfile, token));
        }

        [Fact]
        public async Task CopyAsyncTest()
        {
            var token = A.Dummy<CancellationToken>();

            await fixture.AssetStore.CopyAsync(source, target, token);

            A.CallTo(() => fixture.FtpClient.SetWorkingDirectory("/")).MustHaveHappened();

            A.CallTo(() => fixture.FtpClient.DownloadAsync(A<Stream>._, source, A<long>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();

            A.CallTo(() => fixture.FtpClient.UploadAsync(A<Stream>._, target, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            await fixture.AssetStore.DeleteAsync(source);
            A.CallTo(() => fixture.FtpClient.DeleteFileAsync(source, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void GeneratePublicUrlTest()
        {
            Assert.Null(fixture.AssetStore.GeneratePublicUrl(source));
        }

        [Fact]
        public async Task DownloadAsyncTest()
        {
            var token = A.Dummy<CancellationToken>();

            await fixture.AssetStore.DownloadAsync(source, A.Fake<Stream>(), token);

            A.CallTo(() => fixture.FtpClient.DownloadAsync(A<Stream>._, source, A<long>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();

            A.CallTo(() => fixture.FtpClient.DownloadAsync(A<Stream>._, cannotfindfile, A<long>._, A<IProgress<FtpProgress>>._, token))
                .ThrowsAsync(new FtpException(ftpMessage, new Exception(ftpMessage)));

            await Assert.ThrowsAsync<AssetNotFoundException>(async () => await fixture.AssetStore.DownloadAsync(cannotfindfile, A.Fake<Stream>(), token));
        }

        [Fact]
        public async Task UploadAsyncTest()
        {
            var token = A.Dummy<CancellationToken>();
            await fixture.AssetStore.UploadAsync(source, A.Fake<Stream>(), false, token);
            A.CallTo(() => fixture.FtpClient.UploadAsync(A<Stream>._, source, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();
        }
    }
}
