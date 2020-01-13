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
            AssetStore = new AmazonS3AssetStore(new AmazonS3Options
            {
                AccessKey = "secret",
                Bucket = "squidex-test",
                BucketFolder = "squidex-assets",
                ForcePathStyle = false,
                RegionName = "eu-central-1",
                SecretKey = "secret",
                ServiceUrl = null
            });
            AssetStore.InitializeAsync().Wait();
        }
    }
}
