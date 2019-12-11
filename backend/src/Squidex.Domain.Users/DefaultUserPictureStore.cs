﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Users
{
    public sealed class DefaultUserPictureStore : IUserPictureStore
    {
        private readonly IAssetStore assetStore;

        public DefaultUserPictureStore(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore);

            this.assetStore = assetStore;
        }

        public Task UploadAsync(string userId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(userId);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task DownloadAsync(string userId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(userId);

            return assetStore.DownloadAsync(fileName, stream, ct);
        }

        private static string GetFileName(string userId)
        {
            return $"{userId}_0_picture";
        }
    }
}
