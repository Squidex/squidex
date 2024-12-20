﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class DefaultAppImageStore(
    IAssetStore assetStore,
    IOptions<AssetOptions> options)
    : IAppImageStore
{
    private readonly AssetOptions options = options.Value;

    public Task DownloadAsync(DomainId appId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(appId);

        return assetStore.DownloadAsync(fileName, stream, default, ct);
    }

    public Task UploadAsync(DomainId appId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(appId);

        return assetStore.UploadAsync(fileName, stream, true, ct);
    }

    private string GetFileName(DomainId appId)
    {
        if (options.FolderPerApp)
        {
            return $"{appId}/thumbnail";
        }
        else
        {
            return appId.ToString();
        }
    }
}
