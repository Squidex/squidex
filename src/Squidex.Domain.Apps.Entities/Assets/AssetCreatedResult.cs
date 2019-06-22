// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCreatedResult : AssetResult
    {
        public bool IsDuplicate { get; }

        public AssetCreatedResult(IAssetEntity asset, bool isDuplicate, HashSet<string> tags)
            : base(asset, tags)
        {
            IsDuplicate = isDuplicate;
        }
    }
}
