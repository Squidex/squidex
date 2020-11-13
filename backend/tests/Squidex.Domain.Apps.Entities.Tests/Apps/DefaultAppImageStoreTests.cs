// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class DefaultAppImageStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly string fileName;
        private readonly DefaultAppImageStore sut;

        public DefaultAppImageStoreTests()
        {
            fileName = appId.ToString();

            sut = new DefaultAppImageStore(assetStore);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_archive()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync(appId, stream);

            A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_archive()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(appId, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileName, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }
    }
}
