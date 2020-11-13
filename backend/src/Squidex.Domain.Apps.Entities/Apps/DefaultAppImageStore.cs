// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class DefaultAppImageStore : IAppImageStore
    {
        private readonly IAssetStore assetStore;

        public DefaultAppImageStore(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public Task DownloadAsync(DomainId appId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(appId);

            return assetStore.DownloadAsync(fileName, stream, default, ct);
        }

        public Task UploadAsync(DomainId appId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(appId);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        private static string GetFileName(DomainId appId)
        {
            return appId.ToString();
        }
    }
}
