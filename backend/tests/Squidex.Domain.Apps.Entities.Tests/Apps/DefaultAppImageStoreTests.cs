// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class DefaultAppImageStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly string fileNameDefault;
        private readonly string fileNameFolder;
        private readonly AssetOptions options = new AssetOptions();
        private readonly DefaultAppImageStore sut;

        public DefaultAppImageStoreTests()
        {
            fileNameDefault = appId.ToString();
            fileNameFolder = $"{appId}/thumbnail";

            sut = new DefaultAppImageStore(assetStore, Options.Create(options));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_invoke_asset_store_to_upload_archive(bool folderPerApp)
        {
            var stream = new MemoryStream();

            options.FolderPerApp = folderPerApp;

            var fileName = GetFileName(folderPerApp);

            await sut.UploadAsync(appId, stream);

            A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_invoke_asset_store_to_download_archive(bool folderPerApp)
        {
            var stream = new MemoryStream();

            options.FolderPerApp = folderPerApp;

            var fileName = GetFileName(folderPerApp);

            await sut.DownloadAsync(appId, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileName, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }

        private string GetFileName(bool folderPerApp)
        {
            return folderPerApp ? fileNameFolder : fileNameDefault;
        }
    }
}
