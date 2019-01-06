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
            var url = Sut.GeneratePublicUrl(AssetId, 1, null);

            Assert.Null(url);
        }
    }
}
