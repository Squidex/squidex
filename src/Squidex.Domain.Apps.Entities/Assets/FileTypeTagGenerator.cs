// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class FileTypeTagGenerator : ITagGenerator<CreateAsset>
    {
        public void GenerateTags(CreateAsset source, HashSet<string> tags)
        {
            var extension = source.File?.FileName?.FileType();

            if (!string.IsNullOrWhiteSpace(extension))
            {
                tags.Add($"type/{extension.ToLowerInvariant()}");
            }
        }
    }
}
