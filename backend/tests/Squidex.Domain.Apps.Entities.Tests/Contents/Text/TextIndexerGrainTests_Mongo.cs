﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal class TextIndexerGrainTests_Mongo : TextIndexerGrainTestsBase
    {
        public override IDirectoryFactory DirectoryFactory => CreateFactory();

        private static IDirectoryFactory CreateFactory()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("FullText");

            var mongoBucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = "fs"
            });

            var directoryFactory = new MongoDirectoryFactory(mongoBucket);

            return directoryFactory;
        }
    }
}
