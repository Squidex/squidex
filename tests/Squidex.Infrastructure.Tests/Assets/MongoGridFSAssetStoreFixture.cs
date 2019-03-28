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
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost");
        private readonly IMongoDatabase mongoDatabase;

        public MongoGridFsAssetStore AssetStore { get; }

        public MongoGridFSAssetStoreFixture()
        {
            mongoDatabase = mongoClient.GetDatabase("GridFSTest");

            var gridFSBucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = "fs"
            });

            AssetStore = new MongoGridFsAssetStore(gridFSBucket);
            AssetStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            mongoClient.DropDatabase("GridFSTest");
        }
    }
}
