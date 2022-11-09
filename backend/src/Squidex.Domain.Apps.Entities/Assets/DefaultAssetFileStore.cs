// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class DefaultAssetFileStore : IAssetFileStore, IDeleter
{
    private readonly IAssetStore assetStore;
    private readonly IAssetRepository assetRepository;
    private readonly AssetOptions options;

    public DefaultAssetFileStore(
        IAssetStore assetStore,
        IAssetRepository assetRepository,
        IOptions<AssetOptions> options)
    {
        this.assetStore = assetStore;
        this.assetRepository = assetRepository;

        this.options = options.Value;
    }

    async Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        if (options.FolderPerApp)
        {
            await assetStore.DeleteByPrefixAsync($"{app.Id}/", ct);
        }
        else
        {
            await foreach (var asset in assetRepository.StreamAll(app.Id, ct))
            {
                await DeleteAsync(app.Id, asset.Id, ct);
            }
        }
    }

    public string? GeneratePublicUrl(DomainId appId, DomainId id, long fileVersion, string? suffix)
    {
        var fileName = GetFileName(appId, id, fileVersion, suffix);

        return assetStore.GeneratePublicUrl(fileName);
    }

    public async Task<long> GetFileSizeAsync(DomainId appId, DomainId id, long fileVersion, string? suffix,
        CancellationToken ct = default)
    {
        try
        {
            var fileNameNew = GetFileName(appId, id, fileVersion, suffix);

            return await assetStore.GetSizeAsync(fileNameNew, ct);
        }
        catch (AssetNotFoundException) when (!options.FolderPerApp)
        {
            var fileNameOld = GetFileName(id, fileVersion, suffix);

            return await assetStore.GetSizeAsync(fileNameOld, ct);
        }
    }

    public async Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, BytesRange range = default,
        CancellationToken ct = default)
    {
        try
        {
            var fileNameNew = GetFileName(appId, id, fileVersion, suffix);

            await assetStore.DownloadAsync(fileNameNew, stream, range, ct);
        }
        catch (AssetNotFoundException) when (!options.FolderPerApp)
        {
            var fileNameOld = GetFileName(id, fileVersion, suffix);

            await assetStore.DownloadAsync(fileNameOld, stream, range, ct);
        }
    }

    public Task UploadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, bool overwrite = true,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(appId, id, fileVersion, suffix);

        return assetStore.UploadAsync(fileName, stream, overwrite, ct);
    }

    public Task UploadAsync(string tempFile, Stream stream,
        CancellationToken ct = default)
    {
        return assetStore.UploadAsync(tempFile, stream, false, ct);
    }

    public Task CopyAsync(string tempFile, DomainId appId, DomainId id, long fileVersion, string? suffix,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(appId, id, fileVersion, suffix);

        return assetStore.CopyAsync(tempFile, fileName, ct);
    }

    public Task DeleteAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        if (options.FolderPerApp)
        {
            return assetStore.DeleteByPrefixAsync($"{appId}/{id}", ct);
        }
        else
        {
            var fileNameOld = GetFileName(id);
            var fileNameNew = GetFileName(appId, id);

            return Task.WhenAll(
                assetStore.DeleteByPrefixAsync(fileNameOld, ct),
                assetStore.DeleteByPrefixAsync(fileNameNew, ct));
        }
    }

    public Task DeleteAsync(string tempFile,
        CancellationToken ct = default)
    {
        return assetStore.DeleteAsync(tempFile, ct);
    }

    private string GetFileName(DomainId id, long fileVersion = -1, string? suffix = null)
    {
        return GetFileName(default, id, fileVersion, suffix);
    }

    private string GetFileName(DomainId appId, DomainId id, long fileVersion = -1, string? suffix = null)
    {
        var sb = new StringBuilder(20);

        if (appId != default)
        {
            sb.Append(appId);

            if (options.FolderPerApp)
            {
                sb.Append('/');
            }
            else
            {
                sb.Append('_');
            }
        }

        sb.Append(id);

        if (fileVersion >= 0)
        {
            sb.Append('_');
            sb.Append(fileVersion);
        }

        if (!string.IsNullOrWhiteSpace(suffix))
        {
            sb.Append('_');
            sb.Append(suffix);
        }

        return sb.ToString();
    }
}
