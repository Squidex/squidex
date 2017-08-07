// ==========================================================================
//  GoogleCloudAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Connect();

            var id = Guid.NewGuid().ToString();

            Assert.Equal($"https://storage.cloud.google.com/squidex-test/{id}_1", Sut.GenerateSourceUrl(id, 1, null));
        }
    }
}
