// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class TagAsset : AssetCommand
    {
        public HashSet<string> Tags { get; set; }
    }
}
