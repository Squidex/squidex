// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Squidex.Domain.Users
{
    public sealed class DefaultUserPictureStore : IUserPictureStore
    {
        private readonly IAssetStore assetStore;

        public DefaultUserPictureStore(IAssetStore assetStore)
        {
            this.assetStore = assetStore;
        }

        public string GetPath(string userId)
        {
            return $"{userId}_0_picture";
        }

        public Task UploadAsync(string userId, Stream stream,
            CancellationToken ct = default)
        {
            var fileName = GetPath(userId);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task DownloadAsync(string userId, Stream stream,
            CancellationToken ct = default)
        {
            var fileName = GetPath(userId);

            return assetStore.DownloadAsync(fileName, stream, default, ct);
        }
    }
}
