// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class SvgAssetMetadataSource : IAssetMetadataSource
{
    private const int FileSizeLimit = 2 * 1024 * 1024; // 2MB

    public async Task EnhanceAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        var isSvg =
            command.File.MimeType == "image/svg+xml" ||
            command.File.FileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);

        if (!isSvg)
        {
            return;
        }

        command.Tags.Add("image");

        if (command.File.FileSize >= FileSizeLimit)
        {
            return;
        }

        try
        {
            using (var reader = new StreamReader(command.File.OpenRead()))
            {
                var text = await reader.ReadToEndAsync(ct);

                if (!text.IsValidSvg())
                {
                    throw new ValidationException(T.Get("validation.notAnValidSvg"));
                }

                var (width, height, viewBox) = text.GetSvgMetadata();

                if (!string.IsNullOrWhiteSpace(width) && !string.IsNullOrWhiteSpace(height))
                {
                    command.Metadata[KnownMetadataKeys.SvgWidth] = width;
                    command.Metadata[KnownMetadataKeys.SvgHeight] = height;

                    if (int.TryParse(width, NumberStyles.Integer, CultureInfo.InvariantCulture, out var w) &&
                        int.TryParse(height, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h))
                    {
                        command.Metadata[KnownMetadataKeys.PixelWidth] = w;
                        command.Metadata[KnownMetadataKeys.PixelHeight] = h;
                    }
                }

                if (!string.IsNullOrWhiteSpace(viewBox))
                {
                    command.Metadata[KnownMetadataKeys.SvgViewBox] = viewBox;
                }
            }
        }
        catch (ValidationException)
        {
            throw;
        }
        catch
        {
            return;
        }
    }

    public IEnumerable<string> Format(Asset asset)
    {
        var isSvg =
            asset.MimeType == "image/svg+xml" ||
            asset.FileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);

        if (!isSvg)
        {
            yield break;
        }

        if (asset.Metadata.TryGetString(KnownMetadataKeys.SvgWidth, out var w) &&
            asset.Metadata.TryGetString(KnownMetadataKeys.SvgHeight, out var h))
        {
            yield return $"{w}x{h}";
        }
    }
}
