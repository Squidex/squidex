// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.Assets
{
    [Trait("Category", "Dependencies")]
    public class AmazonS3AssetStoreTests : AssetStoreTests<AmazonS3AssetStore>, IClassFixture<AmazonS3AssetStoreFixture>
    {
        public AmazonS3AssetStoreFixture _ { get; }

        public AmazonS3AssetStoreTests(AmazonS3AssetStoreFixture fixture)
        {
            _ = fixture;
        }

        public override AmazonS3AssetStore CreateStore()
        {
            return _.AssetStore;
        }

        [Fact]
        public async Task Should_throw_exception_for_invalid_config()
        {
            var sut = new AmazonS3AssetStore(new AmazonS3Options
            {
                ServiceUrl = null,
                RegionName = "invalid",
                Bucket = "invalid",
                BucketFolder = null,
                AccessKey = "invalid",
                SecretKey = "invalid",
                ForcePathStyle = false
            });

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
