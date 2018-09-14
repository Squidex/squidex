// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Xunit;

#pragma warning disable xUnit1000 // Test classes must be public

namespace Squidex.Infrastructure.Assets
{
    internal class MongoGridFSAssetStoreTests : AssetStoreTests<MongoGridFsAssetStore>
    {
        private static readonly IGridFSBucket<string> GridFSBucket;

        static MongoGridFSAssetStoreTests()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("Test");

            GridFSBucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = "fs"
            });
        }

        public override MongoGridFsAssetStore CreateStore()
        {
            return new MongoGridFsAssetStore(GridFSBucket);
        }

        public override void Dispose()
        {
        }

        [Fact]
        public void Should_not_calculate_source_url()
        {
            Assert.Equal("UNSUPPORTED", Sut.GenerateSourceUrl(AssetId, 1, null));
        }
    }
}