// ==========================================================================
//  GoogleCloudAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    internal class GoogleCloudAssetStoreTests : AssetStoreTests<GoogleCloudAssetStore>
    {
        public override GoogleCloudAssetStore CreateStore()
        {
            return new GoogleCloudAssetStore("squidex-test");
        }

        public override void Dispose()
        {
        }
    }
}
