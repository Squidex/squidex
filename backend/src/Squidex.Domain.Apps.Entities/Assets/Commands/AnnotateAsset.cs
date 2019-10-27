// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class AnnotateAsset : AssetCommand
    {
        public string FileName { get; set; }

        public string Slug { get; set; }

        public HashSet<string> Tags { get; set; }
    }
}
