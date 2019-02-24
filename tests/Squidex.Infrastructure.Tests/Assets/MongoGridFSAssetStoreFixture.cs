// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Squidex.Infrastructure.Assets
{
    public sealed class MongoGridFSAssetStoreFixture : IDisposable
    {
        public MongoGridFsAssetStore AssetStore { get; }

        public IMongoClient MongoClient { get; } = new MongoClient("mongodb://localhost");

        public IMongoDatabase MongoDatabase { get; }

        public MongoGridFSAssetStoreFixture()
        {
            MongoDatabase = MongoClient.GetDatabase("GridFSTest");

            var gridFSBucket = new GridFSBucket<string>(MongoDatabase, new GridFSBucketOptions
            {
                BucketName = "fs"
            });

            AssetStore = new MongoGridFsAssetStore(gridFSBucket);
            AssetStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            MongoClient.DropDatabase("GridFSTest");
        }
    }
}
