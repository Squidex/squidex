// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene.Storage;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;
using Squidex.Infrastructure.Assets;

namespace Benchmarks
{
    public static class IndexStorages
    {
        public static IIndexStorage Assets()
        {
            var storage = new AssetIndexStorage(new MemoryAssetStore());

            return storage;
        }

        public static IIndexStorage TempFolder()
        {
            var storage = new FileIndexStorage();

            return storage;
        }

        public static IIndexStorage MongoDB()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("FullText");

            var mongoBucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = $"bucket_{DateTime.UtcNow.Ticks}"
            });

            var storage = new MongoIndexStorage(mongoBucket);

            return storage;
        }
    }
}
