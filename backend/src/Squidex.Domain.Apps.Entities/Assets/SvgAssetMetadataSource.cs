// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        if (isSvg)
        {
            command.Tags.Add("image");

            if (command.File.FileSize < FileSizeLimit)
            {
                try
                {
                    using (var reader = new StreamReader(command.File.OpenRead()))
                    {
                        var text = await reader.ReadToEndAsync();

                        if (!text.IsValidSvg())
                        {
                            throw new ValidationException(T.Get("validation.notAnValidSvg"));
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
        }
    }

    public IEnumerable<string> Format(IAssetEntity asset)
    {
        yield break;
    }
}
