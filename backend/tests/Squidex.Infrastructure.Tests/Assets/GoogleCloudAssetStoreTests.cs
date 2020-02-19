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
    public class GoogleCloudAssetStoreTests : AssetStoreTests<GoogleCloudAssetStore>, IClassFixture<GoogleCloudAssetStoreFixture>
    {
        public GoogleCloudAssetStoreFixture _ { get; }

        public GoogleCloudAssetStoreTests(GoogleCloudAssetStoreFixture fixture)
        {
            _ = fixture;
        }

        public override GoogleCloudAssetStore CreateStore()
        {
            return _.AssetStore;
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }
    }
}
