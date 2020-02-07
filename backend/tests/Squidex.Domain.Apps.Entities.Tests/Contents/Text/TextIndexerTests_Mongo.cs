// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal class TextIndexerTests_Mongo : TextIndexerTestsBase
    {
        public override IIndexerFactory Factory { get; } = new LuceneIndexFactory(CreateStorage());

        private static IIndexStorage CreateStorage()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("FullText");

            var mongoBucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = "fs"
            });

            var storage = new MongoIndexStorage(mongoBucket);

            return storage;
        }
    }
}
