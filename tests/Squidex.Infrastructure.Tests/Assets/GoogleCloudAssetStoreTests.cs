// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Assets
{
    [Trait("Dependency", "GC")]
    public class GoogleCloudAssetStoreTests : AssetStoreTests<GoogleCloudAssetStore>, IClassFixture<GoogleCloudAssetStoreFixture>
    {
        private readonly GoogleCloudAssetStoreFixture fixture;

        public GoogleCloudAssetStoreTests(GoogleCloudAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override GoogleCloudAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(AssetId, 1, null);

            Assert.Null(url);
        }
    }
}
