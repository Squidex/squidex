// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    public sealed class AzureBlobAssetStoreFixture
    {
        public AzureBlobAssetStore AssetStore { get; }

        public AzureBlobAssetStoreFixture()
        {
            AssetStore = new AzureBlobAssetStore("UseDevelopmentStorage=true", "squidex-test-container");
            AssetStore.InitializeAsync().Wait();
        }
    }
}
