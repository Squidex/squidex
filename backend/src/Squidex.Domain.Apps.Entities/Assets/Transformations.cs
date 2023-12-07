// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Assets;

public record struct AssetRef(
    NamedId<DomainId> AppId,
    DomainId Id,
    long FileVersion,
    long FileSize,
    string MimeType,
    string? FileId,
    AssetType Type);

public static class Transformations
{
    private const int MaxSize = 4 * 1024 * 1024;
    private const string ErrorNoAsset = "NoAsset";
    private const string ErrorTooBig = "ErrorTooBig";

    public static AssetRef ToRef(this EnrichedAssetEvent @event)
    {
        return new AssetRef(
            @event.AppId,
            @event.Id,
            @event.FileVersion,
            @event.FileSize,
            @event.MimeType,
            null,
            @event.AssetType);
    }

    public static AssetRef ToRef(this Asset asset)
    {
        return new AssetRef(
            asset.AppId,
            asset.Id,
            asset.FileVersion,
            asset.FileSize,
            asset.MimeType,
            null,
            asset.Type);
    }

    public static async Task<string> GetTextAsync(this AssetRef asset, string? encoding, IServiceProvider services,
        CancellationToken ct = default)
    {
        if (asset == default)
        {
            return ErrorNoAsset;
        }

        if (asset.FileSize > MaxSize)
        {
            return ErrorTooBig;
        }

        var assetFileStore = services.GetRequiredService<IAssetFileStore>();

        using (var stream = DefaultPools.MemoryStream.GetStream())
        {
            await DownloadAsync(asset, assetFileStore, stream, ct);

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

    public static async Task<string?> GetBlurHashAsync(this AssetRef asset, BlurOptions options, IServiceProvider services,
        CancellationToken ct = default)
    {
        if (asset == default)
        {
            return ErrorNoAsset;
        }

        if (asset.FileSize > MaxSize || asset.Type != AssetType.Image)
        {
            return null;
        }

        var assetFileStore = services.GetRequiredService<IAssetFileStore>();
        var assetGenerator = services.GetRequiredService<IAssetThumbnailGenerator>();

        using (var stream = DefaultPools.MemoryStream.GetStream())
        {
            await DownloadAsync(asset, assetFileStore, stream, ct);

            return await assetGenerator.ComputeBlurHashAsync(stream, asset.MimeType, options, ct);
        }
    }

    private static async Task DownloadAsync(AssetRef asset, IAssetFileStore assetFileStore, MemoryStream stream, CancellationToken ct)
    {
        if (asset.FileId != null)
        {
            await assetFileStore.DownloadAsync(asset.FileId, stream, ct);
        }
        else
        {
            await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, null, stream, default, ct);
        }

        stream.Position = 0;
    }
}
