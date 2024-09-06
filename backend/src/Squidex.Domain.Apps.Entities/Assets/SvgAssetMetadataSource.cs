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
                    var hasNumericWidth = TryParseInt(width, out var w);
                    var hasNumericHeight = TryParseInt(height, out var h);

                    if (hasNumericWidth)
                    {
                        command.Metadata[KnownMetadataKeys.SvgWidth] = w;
                    }
                    else
                    {
                        command.Metadata[KnownMetadataKeys.SvgWidth] = width;
                    }

                    if (hasNumericWidth)
                    {
                        command.Metadata[KnownMetadataKeys.SvgHeight] = h;
                    }
                    else
                    {
                        command.Metadata[KnownMetadataKeys.SvgHeight] = height;
                    }

                    if (hasNumericWidth && hasNumericHeight)
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

    private static bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
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

        if (asset.Metadata.TryGetValue(KnownMetadataKeys.SvgWidth, out var w) &&
            asset.Metadata.TryGetValue(KnownMetadataKeys.SvgHeight, out var h))
        {
            yield return $"{w}x{h}";
        }
    }
}
