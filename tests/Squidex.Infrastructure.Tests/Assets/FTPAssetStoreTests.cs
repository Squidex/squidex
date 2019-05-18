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
    public class FTPAssetStoreTests : AssetStoreTests<FTPAssetStore>, IClassFixture<FTPAssetStoreFixture>
    {
        private readonly FTPAssetStoreFixture fixture;

        public FTPAssetStoreTests(FTPAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Should_call_4_methods()
        {
            var token = A.Dummy<CancellationToken>();
            await fixture.AssetStore.InitializeAsync(token);

            A.CallTo(() => fixture.FtpClient.ConnectAsync(token)).MustHaveHappened();
            A.CallTo(() => fixture.FtpClient.DirectoryExistsAsync("/", token)).Returns(false);
            A.CallTo(() => fixture.FtpClient.ConnectAsync(token)).MustHaveHappened();
            A.CallTo(() => fixture.FtpClient.CreateDirectoryAsync("/", token)).MustHaveHappened();
        }

        public override FTPAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public async Task Should_throw_exception_when_copy_files()
        {
            var token = A.Dummy<CancellationToken>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(null, fixture.Target, token));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(fixture.Source, null, token));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await fixture.AssetStore.CopyAsync(null, null, token));

            await Assert.ThrowsAsync<AssetNotFoundException>(async () => await fixture.AssetStore.CopyAsync(fixture.Cannotfindfile, fixture.Target, token));
        }

        [Fact]
        public async Task Should_call_copy_method()
        {
            var token = A.Dummy<CancellationToken>();

            await fixture.AssetStore.CopyAsync(fixture.Source, fixture.Target, token);

            A.CallTo(() => fixture.FtpClient.SetWorkingDirectory("/")).MustHaveHappened();

            A.CallTo(() => fixture.FtpClient.DownloadAsync(A<Stream>._, fixture.Source, A<long>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();

            A.CallTo(() => fixture.FtpClient.UploadAsync(A<Stream>._, fixture.Target, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_delete_async()
        {
            await fixture.AssetStore.DeleteAsync(fixture.Source);
            A.CallTo(() => fixture.FtpClient.DeleteFileAsync(fixture.Source, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void Should_return_null_value()
        {
            Assert.Null(fixture.AssetStore.GeneratePublicUrl(fixture.Source));
        }

        [Fact]
        public async Task Should_call_download_async_methods()
        {
            var token = A.Dummy<CancellationToken>();

            await fixture.AssetStore.DownloadAsync(fixture.Source, A.Fake<Stream>(), token);

            A.CallTo(() => fixture.FtpClient.DownloadAsync(A<Stream>._, fixture.Source, A<long>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();

            await Assert.ThrowsAsync<AssetNotFoundException>(async () => await fixture.AssetStore.DownloadAsync(fixture.Cannotfindfile, A.Fake<Stream>(), token));
        }

        [Fact]
        public async Task Should_call_upload_async_method()
        {
            var token = A.Dummy<CancellationToken>();
            await Assert.ThrowsAsync<AssetAlreadyExistsException>(async () => await fixture.AssetStore.UploadAsync(fixture.Source, A.Fake<Stream>(), false, token));
            A.CallTo(() => fixture.FtpClient.UploadAsync(A<Stream>._, fixture.Source, A<FtpExists>._, A<bool>._, A<IProgress<FtpProgress>>._, token)).MustHaveHappened();
        }
    }
}
