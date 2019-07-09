// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Assets
{
    [Trait("Category", "Dependencies")]
    public class FTPAssetStoreTests : AssetStoreTests<FTPAssetStore>, IClassFixture<FTPAssetStoreFixture>
    {
        private readonly FTPAssetStoreFixture fixture;

        public FTPAssetStoreTests(FTPAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override FTPAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }
    }
}
