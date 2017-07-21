// ==========================================================================
//  AzureBlobAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Azure.Storage;

namespace Squidex.Infrastructure.Assets
{
    public class AzureBlobAssetStoreTests : AssetStoreTests<AzureBlobAssetStore>
    {
        public override AzureBlobAssetStore CreateStore()
        {
            var azureStorageAccount =
                new StorageAccountManager("UseDevelopmentStorage=true");

            return new AzureBlobAssetStore(azureStorageAccount, "squidex-test-container"); ;
        }

        public override void Dispose()
        {
        }
    }
}
