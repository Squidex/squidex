// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoDirectoryFactory : IDirectoryFactory
    {
        private readonly IGridFSBucket<string> bucket;

        public MongoDirectoryFactory(IGridFSBucket<string> bucket)
        {
            this.bucket = bucket;
        }

        public LuceneDirectory Create(Guid schemaId)
        {
            var folderName = schemaId.ToString();

            var tempFolder = Path.Combine(Path.GetTempPath(), folderName);
            var tempDirectory = new DirectoryInfo(tempFolder);

            return new MongoDirectory(bucket, folderName, tempDirectory);
        }
    }
}
