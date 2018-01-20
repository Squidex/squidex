// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Users
{
    public sealed class AssetUserPictureStore : IUserPictureStore
    {
        private readonly IAssetStore assetStore;

        public AssetUserPictureStore(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public Task UploadAsync(string userId, Stream stream)
        {
            return assetStore.UploadAsync(userId, 0, "picture", stream);
        }

        public async Task<Stream> DownloadAsync(string userId)
        {
            var memoryStream = new MemoryStream();

            await assetStore.DownloadAsync(userId, 0, "picture", memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
