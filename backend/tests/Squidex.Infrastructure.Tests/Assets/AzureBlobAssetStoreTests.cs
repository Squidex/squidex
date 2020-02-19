// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.Assets
{
    [Trait("Category", "Dependencies")]
    public class AzureBlobAssetStoreTests : AssetStoreTests<AzureBlobAssetStore>, IClassFixture<AzureBlobAssetStoreFixture>
    {
        public AzureBlobAssetStoreFixture _ { get; }

        public AzureBlobAssetStoreTests(AzureBlobAssetStoreFixture fixture)
        {
            _ = fixture;
        }

        public override AzureBlobAssetStore CreateStore()
        {
            return _.AssetStore;
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Equal($"http://127.0.0.1:10000/devstoreaccount1/squidex-test-container/{FileName}", url);
        }
    }
}
