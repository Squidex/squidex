// ==========================================================================
//  AzureBlobAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

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

        [Fact]
        public void Should_calculate_source_url()
        {
            Sut.Connect();

            var id = Guid.NewGuid().ToString();

            Assert.Equal($"http://127.0.0.1:10000/squidex-test-container/{id}_1", Sut.GenerateSourceUrl(id, 1, null));
        }
    }
}
