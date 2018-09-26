// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

#pragma warning disable xUnit1000 // Test classes must be public

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
            var url = Sut.GenerateSourceUrl(AssetId, 1, null);

            Assert.Equal($"http://127.0.0.1:10000/squidex-test-container/{AssetId}_1", url);
        }
    }
}
