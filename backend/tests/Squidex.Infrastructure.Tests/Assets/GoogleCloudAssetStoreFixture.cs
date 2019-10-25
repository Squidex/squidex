// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Assets
{
    public sealed class GoogleCloudAssetStoreFixture : IDisposable
    {
        public GoogleCloudAssetStore AssetStore { get; }

        public GoogleCloudAssetStoreFixture()
        {
            AssetStore = new GoogleCloudAssetStore("squidex-test");
            AssetStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
        }
    }
}
