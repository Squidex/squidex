// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Xunit;

#pragma warning disable xUnit1000 // Test classes must be public

namespace Squidex.Infrastructure.Assets
{
    internal class MongoGridFSAssetStoreTests : AssetStoreTests<MongoGridFsAssetStore>
    {
        private static readonly IMongoClient MongoClient;
        private static readonly IMongoDatabase MongoDatabase;
        private static readonly IGridFSBucket<string> GridFSBucket;

        static MongoGridFSAssetStoreTests()
        {
            MongoClient = new MongoClient("mongodb://localhost");
            MongoDatabase = MongoClient.GetDatabase("Test");

            GridFSBucket = new GridFSBucket<string>(MongoDatabase, new GridFSBucketOptions
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
            Sut.Initialize();

            Assert.Equal("UNSUPPORTED", Sut.GenerateSourceUrl(Guid.NewGuid().ToString(), 1, null));
        }
    }
}