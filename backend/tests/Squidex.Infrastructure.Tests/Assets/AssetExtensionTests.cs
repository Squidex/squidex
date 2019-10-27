// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class AssetExtensionTests
    {
        private readonly IAssetStore sut = A.Fake<IAssetStore>();
        private readonly Guid id = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly string fileName = Guid.NewGuid().ToString();

        [Fact]
        public void Should_copy_with_id_and_version()
        {
            sut.CopyAsync(fileName, id, 1, string.Empty);

            A.CallTo(() => sut.CopyAsync(fileName, $"{id}_1", default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_copy_with_id_and_version_and_suffix()
        {
            sut.CopyAsync(fileName, id, 1, "Crop");

            A.CallTo(() => sut.CopyAsync(fileName, $"{id}_1_Crop", default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_upload_with_id_and_version()
        {
            sut.UploadAsync(id, 1, string.Empty, stream, true);

            A.CallTo(() => sut.UploadAsync($"{id}_1", stream, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_upload_with_id_and_version_and_suffix()
        {
            sut.UploadAsync(id, 1, "Crop", stream, true);

            A.CallTo(() => sut.UploadAsync($"{id}_1_Crop", stream, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_download_with_id_and_version()
        {
            sut.DownloadAsync(id, 1, string.Empty, stream);

            A.CallTo(() => sut.DownloadAsync($"{id}_1", stream, default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_download_with_id_and_version_and_suffix()
        {
            sut.DownloadAsync(id, 1, "Crop", stream);

            A.CallTo(() => sut.DownloadAsync($"{id}_1_Crop", stream, default))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_delete_with_id_and_version()
        {
            sut.DeleteAsync(id, 1, string.Empty);

            A.CallTo(() => sut.DeleteAsync($"{id}_1"))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_delete_with_id_and_version_and_suffix()
        {
            sut.DeleteAsync(id, 1, "Crop");

            A.CallTo(() => sut.DeleteAsync($"{id}_1_Crop"))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_generate_url_with_id_and_version()
        {
            sut.GeneratePublicUrl(id, 1, string.Empty);

            A.CallTo(() => sut.GeneratePublicUrl($"{id}_1"))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_generate_url_with_id_and_version_and_suffix()
        {
            sut.GeneratePublicUrl(id, 1, "Crop");

            A.CallTo(() => sut.GeneratePublicUrl($"{id}_1_Crop"))
                .MustHaveHappened();
        }
    }
}
