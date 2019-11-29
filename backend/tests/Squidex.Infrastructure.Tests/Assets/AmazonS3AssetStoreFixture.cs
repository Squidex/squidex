// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    public sealed class AmazonS3AssetStoreFixture
    {
        public AmazonS3AssetStore AssetStore { get; }

        public AmazonS3AssetStoreFixture()
        {
            AssetStore = new AmazonS3AssetStore("eu-central-1", "squidex-test", "squidex-assets", "secret", "secret");
            AssetStore.InitializeAsync().Wait();
        }
    }
}
