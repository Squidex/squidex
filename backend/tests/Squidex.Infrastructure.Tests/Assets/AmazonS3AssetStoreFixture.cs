﻿// ==========================================================================
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
            AssetStore = new AmazonS3AssetStore(new MyAmazonS3Options
            {
                ServiceUrl = null,
                RegionName = "eu-central-1",
                Bucket = "squidex-test",
                BucketFolder = "squidex-assets",
                AccessKey = "secret",
                SecretKey = "secret",
                ForcePathStyle = false
            });
            AssetStore.InitializeAsync().Wait();
        }
    }
}
