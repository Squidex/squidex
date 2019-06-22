// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetResult
    {
        public IAssetEntity Asset { get; }

        public HashSet<string> Tags { get; }

        public AssetResult(IAssetEntity asset, HashSet<string> tags)
        {
            Asset = asset;

            Tags = tags;
        }
    }
}
