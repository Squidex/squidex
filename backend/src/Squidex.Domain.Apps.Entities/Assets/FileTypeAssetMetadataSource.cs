﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class FileTypeAssetMetadataSource : IAssetMetadataSource
    {
        public Task EnhanceAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            if (tags != null)
            {
                var extension = command.File?.FileName?.FileType();

                if (!string.IsNullOrWhiteSpace(extension))
                {
                    tags.Add($"type/{extension.ToLowerInvariant()}");
                }
            }

            return TaskHelper.Done;
        }

        public IEnumerable<string> Format(IAssetEntity asset)
        {
            yield break;
        }
    }
}
