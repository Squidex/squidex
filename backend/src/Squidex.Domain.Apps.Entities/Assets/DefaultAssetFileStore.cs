// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class DefaultAssetFileStore(
    IAssetStore assetStore,
    IAssetRepository assetRepository,
    IMemoryCache cache,
    IOptions<AssetOptions> options)
    : IAssetFileStore, IDeleter
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private readonly AssetOptions options = options.Value;

    async Task IDeleter.DeleteAppAsync(App app,
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
        if (options.FolderPerApp)
        {
            return await assetStore.GetSizeAsync(GetFileName(appId, id, fileVersion, suffix), ct);
        }

        var (first, second, isOldFirst) = FileNames(appId, id, fileVersion, suffix);
        try
        {
            return await assetStore.GetSizeAsync(first, ct);
        }
        catch (AssetNotFoundException)
        {
            RememberFileName(appId, id, !isOldFirst);

            return await assetStore.GetSizeAsync(second, ct);
        }
    }

    public async Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, BytesRange range = default,
        CancellationToken ct = default)
    {
        if (options.FolderPerApp)
        {
            await assetStore.DownloadAsync(GetFileName(appId, id, fileVersion, suffix), stream, range, ct);
            return;
        }

        var (first, second, isOldFirst) = FileNames(appId, id, fileVersion, suffix);
        try
        {
            await assetStore.DownloadAsync(first, stream, range, ct);
        }
        catch (AssetNotFoundException)
        {
            RememberFileName(appId, id, !isOldFirst);

            await assetStore.DownloadAsync(second, stream, range, ct);
        }
    }

    // Assets from older versions are stored under a file name without the app ID. Which name an
    // asset uses can only be found out by trying, and a failed try is a full roundtrip to the asset
    // store. Therefore the outcome is remembered as a hint. Both names are still tried, so that a
    // stale hint only costs the roundtrip it was there to save.
    private (string First, string Second, bool IsOldFirst) FileNames(DomainId appId, DomainId id, long fileVersion, string? suffix)
    {
        var fileNameNew = GetFileName(appId, id, fileVersion, suffix);
        var fileNameOld = GetFileName(id, fileVersion, suffix);

        if (cache.TryGetValue<bool>(CacheKey(appId, id), out var useOld) && useOld)
        {
            return (fileNameOld, fileNameNew, true);
        }

        return (fileNameNew, fileNameOld, false);
    }

    private void RememberFileName(DomainId appId, DomainId id, bool useOld)
    {
        cache.Set(CacheKey(appId, id), useOld, CacheDuration);
    }

    private static object CacheKey(DomainId appId, DomainId id)
    {
        return (typeof(DefaultAssetFileStore), appId, id);
    }

    public Task DownloadAsync(string tempFile, Stream stream,
        CancellationToken ct = default)
    {
        return assetStore.DownloadAsync(tempFile, stream, default, ct);
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
