// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Tags;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class ImageTagGenerator : ITagGenerator<CreateAsset>
    {
        public void GenerateTags(CreateAsset source, HashSet<string> tags)
        {
            if (source.ImageInfo != null)
            {
                tags.Add("image");

                var wh = source.ImageInfo.PixelWidth + source.ImageInfo.PixelHeight;

                if (wh > 2000)
                {
                    tags.Add("image/large");
                }
                else if (wh > 1000)
                {
                    tags.Add("image/medium");
                }
                else
                {
                    tags.Add("image/small");
                }
            }
        }
    }
}
