// ==========================================================================
//  AzureBlobAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    internal class AzureBlobAssetStoreTests : AssetStoreTests<AzureBlobAssetStore>
    {
        public override AzureBlobAssetStore CreateStore()
        {
            return new AzureBlobAssetStore("UseDevelopmentStorage=true", "squidex-test-container");
        }

        public override void Dispose()
        {
        }
    }
}
