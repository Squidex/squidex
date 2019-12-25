// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    [Trait("Category", "Dependencies")]
    public class AmazonS3AssetStoreTests : AssetStoreTests<AmazonS3AssetStore>, IClassFixture<AmazonS3AssetStoreFixture>
    {
        private readonly AmazonS3AssetStoreFixture fixture;

        public AmazonS3AssetStoreTests(AmazonS3AssetStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override AmazonS3AssetStore CreateStore()
        {
            return fixture.AssetStore;
        }

        [Fact]
        public async Task Should_throw_exception_for_invalid_config()
        {
            var sut = new AmazonS3AssetStore(null, "invalid", "invalid", null, "invalid", "invalid");

            await Assert.ThrowsAsync<ConfigurationException>(() => sut.InitializeAsync());
        }

        [Fact]
        public void Should_calculate_source_url()
        {
            var url = Sut.GeneratePublicUrl(FileName);

            Assert.Null(url);
        }
    }
}
