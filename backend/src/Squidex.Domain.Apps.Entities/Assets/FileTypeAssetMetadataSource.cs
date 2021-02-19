// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class FileTypeAssetMetadataSource : IAssetMetadataSource
    {
        public Task EnhanceAsync(UploadAssetCommand command)
        {
            if (command.Tags != null)
            {
                var extension = command.File?.FileName?.FileType();

                if (!string.IsNullOrWhiteSpace(extension))
                {
                    command.Tags.Add($"type/{extension.ToLowerInvariant()}");
                }
            }

            return Task.CompletedTask;
        }

        public IEnumerable<string> Format(IAssetEntity asset)
        {
            yield break;
        }
    }
}
