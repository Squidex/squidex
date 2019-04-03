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
    public class MongoGridFsAssetStoreTests : AssetStoreTests<MongoGridFsAssetStore>, IClassFixture<MongoGridFSAssetStoreFixture>
    {
        private readonly MongoGridFSAssetStoreFixture fixture;

        public MongoGridFsAssetStoreTests(MongoGridFSAssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override MongoGridFsAssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public void Should_not_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }
    }
}