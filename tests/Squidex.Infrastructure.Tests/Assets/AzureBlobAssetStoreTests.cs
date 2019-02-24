// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Assets
{
    [Trait("Dependency", "Azure")]
    public class AzureBlobAssetStoreTests : AssetStoreTests<AzureBlobAssetStore>, IClassFixture<AzureBlobAssetStoreFixture>
    {
        private readonly AzureBlobAssetStoreFixture fixture;

        public AzureBlobAssetStoreTests(AzureBlobAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override AzureBlobAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(AssetId, 1, null);

            Assert.Equal($"http://127.0.0.1:10000/devstoreaccount1/squidex-test-container/{AssetId}_1", url);
        }
    }
}
