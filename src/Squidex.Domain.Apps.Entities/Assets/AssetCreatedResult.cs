// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCreatedResult
    {
        public IEnrichedAssetEntity Asset { get; }

        public bool IsDuplicate { get; }

        public AssetCreatedResult(IEnrichedAssetEntity asset, bool isDuplicate)
        {
            Asset = asset;

            IsDuplicate = isDuplicate;
        }
    }
}
