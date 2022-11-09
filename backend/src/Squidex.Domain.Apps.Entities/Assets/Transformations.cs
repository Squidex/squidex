// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Assets;

public record struct AssetRef(
    DomainId AppId,
    DomainId Id,
    long FileVersion,
    long FileSize,
    string MimeType,
    AssetType Type);

public static class Transformations
{
    public static AssetRef ToRef(this EnrichedAssetEvent @event)
    {
        return new AssetRef(
            @event.AppId.Id,
            @event.Id,
            @event.FileVersion,
            @event.FileSize,
            @event.MimeType,
            @event.AssetType);
    }

    public static AssetRef ToRef(this IAssetEntity asset)
    {
        return new AssetRef(
            asset.AppId.Id,
            asset.Id,
            asset.FileVersion,
            asset.FileSize,
            asset.MimeType,
            asset.Type);
    }

    public static async Task<string> GetTextAsync(this AssetRef asset, string? encoding,
        IAssetFileStore assetFileStore,
        CancellationToken ct = default)
    {
        using (var stream = DefaultPools.MemoryStream.GetStream())
        {
            await assetFileStore.DownloadAsync(asset.AppId, asset.Id, asset.FileVersion, null, stream, default, ct);

            stream.Position = 0;

            var bytes = stream.ToArray();

            switch (encoding?.ToLowerInvariant())
            {
                case "base64":
                    return Convert.ToBase64String(bytes);
                case "ascii":
                    return Encoding.ASCII.GetString(bytes);
                case "unicode":
                    return Encoding.Unicode.GetString(bytes);
                default:
                    return Encoding.UTF8.GetString(bytes);
            }
        }
    }

    public static async Task<string?> GetBlurHashAsync(this AssetRef asset, BlurOptions options,
        IAssetFileStore assetFileStore,
        IAssetThumbnailGenerator assetThumbnails,
        CancellationToken ct = default)
    {
        using (var stream = DefaultPools.MemoryStream.GetStream())
        {
            await assetFileStore.DownloadAsync(asset.AppId, asset.Id, asset.FileVersion, null, stream, default, ct);

            stream.Position = 0;

            return await assetThumbnails.ComputeBlurHashAsync(stream, asset.MimeType, options, ct);
        }
    }
}
