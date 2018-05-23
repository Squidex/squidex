// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using FakeItEasy;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class MongoGridFsAssetStoreTests : AssetStoreTests<MongoGridFsAssetStore>
    {
        private readonly string testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private readonly IGridFSBucket<string> bucket = A.Fake<IGridFSBucket<string>>();

        public override MongoGridFsAssetStore CreateStore()
        {
            return new MongoGridFsAssetStore(bucket, testFolder);
        }

        public override void Dispose()
        {
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Initialize();

            var id = Id();

            Assert.Equal(Path.Combine(testFolder, $"{id}_1"), Sut.GenerateSourceUrl(id, 1, null));
        }

        [Fact]
        public override Task Should_throw_exception_if_asset_to_download_is_not_found()
        {
            var id = Id();
            var filename = $"{id}_1_suffix";

            A.CallTo(() =>
                    bucket.DownloadToStreamAsync(filename, A<MemoryStream>.Ignored, null,
                        A<CancellationToken>.Ignored))
                .Throws<AssetNotFoundException>();

            ((IInitializable)Sut).Initialize();

            return Assert.ThrowsAsync<AssetNotFoundException>(() =>
                Sut.DownloadAsync(id, 1, "suffix", new MemoryStream()));
        }

        [Fact(Skip = "GridFSDownloadStream and GridFSUploadStream are not mockable")]
        public override Task Should_commit_temporary_file()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Should_try_to_download_asset_if_is_not_found_locally()
        {
            var id = Id();
            var filename = $"{id}_1_suffix";
            using (var stream = new MemoryStream())
            {
                ((IInitializable)Sut).Initialize();

                await Sut.DownloadAsync(id, 1, "suffix", stream);

                A.CallTo(() =>
                        bucket.DownloadToStreamAsync(filename, stream,
                            A<GridFSDownloadByNameOptions>.Ignored,
                            A<CancellationToken>.Ignored))
                    .MustHaveHappened();
            }
        }

        [Fact]
        public async Task Should_try_to_upload_asset_and_save_locally()
        {
            var id = Id();
            var filename = $"{id}_1_suffix";
            var file = new FileInfo(testFolder + Path.DirectorySeparatorChar + filename);
            using (var stream = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 }))
            {
                ((IInitializable)Sut).Initialize();

                await Sut.UploadAsync(id, 1, "suffix", stream);

                A.CallTo(() =>
                        bucket.UploadFromStreamAsync(filename, filename, stream, A<GridFSUploadOptions>.Ignored,
                            A<CancellationToken>.Ignored))
                    .MustHaveHappened();

                Assert.True(file.Exists);
            }
        }
    }
}