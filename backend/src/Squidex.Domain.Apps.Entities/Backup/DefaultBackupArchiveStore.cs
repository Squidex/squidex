// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed class DefaultBackupArchiveStore(IAssetStore assetStore) : IBackupArchiveStore
{
    public Task DownloadAsync(DomainId backupId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(backupId);

        return assetStore.DownloadAsync(fileName, stream, default, ct);
    }

    public Task UploadAsync(DomainId backupId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(backupId);

        return assetStore.UploadAsync(fileName, stream, true, ct);
    }

    public Task DeleteAsync(DomainId backupId,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(backupId);

        return assetStore.DeleteAsync(fileName, ct);
    }

    private static string GetFileName(DomainId backupId)
    {
        return $"{backupId}_0";
    }
}
