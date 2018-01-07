// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Xunit;

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

        // [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Initialize();

            var id = Guid.NewGuid().ToString();

            Assert.Equal($"https://storage.cloud.google.com/squidex-test/{id}_1", Sut.GenerateSourceUrl(id, 1, null));
        }
    }
}
