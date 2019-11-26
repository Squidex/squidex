// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Lucene.Net.Store;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoIndexOutput : IndexOutput
    {
        private readonly IndexOutput cacheOutput;
        private readonly MongoDirectory indexDirectory;
        private readonly string indexName;
        private bool isFlushed;
        private bool isWritten;

        public override long Length
        {
            get { return cacheOutput.Length; }
        }

        public override long Checksum
        {
            get { return cacheOutput.Checksum; }
        }

        public MongoIndexOutput(MongoDirectory indexDirectory, IOContext context, string indexName)
        {
            this.indexDirectory = indexDirectory;
            this.indexName = indexName;

            cacheOutput = indexDirectory.CacheDirectory.CreateOutput(indexName, context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Flush();

                cacheOutput.Dispose();

                if (isWritten && isFlushed)
                {
                    var fileInfo = new FileInfo(indexDirectory.GetFullPath(indexName));

                    using (var fs = new FileStream(indexDirectory.GetFullPath(indexName), FileMode.Open, FileAccess.Read))
                    {
                        var fullName = indexDirectory.GetFullName(indexName);

                        var options = new GridFSUploadOptions
                        {
                            Metadata = new BsonDocument
                            {
                                ["WrittenTime"] = fileInfo.LastWriteTimeUtc
                            }
                        };

                        indexDirectory.Bucket.UploadFromStream(fullName, indexName, fs, options);
                    }
                }
            }
        }

        public override long GetFilePointer()
        {
            return cacheOutput.GetFilePointer();
        }

        public override void Flush()
        {
            cacheOutput.Flush();

            isFlushed = true;
        }

        public override void WriteByte(byte b)
        {
            cacheOutput.WriteByte(b);

            isWritten = true;
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            cacheOutput.WriteBytes(b, offset, length);

            isWritten = true;
        }

        [Obsolete]
        public override void Seek(long pos)
        {
            cacheOutput.Seek(pos);
        }
    }
}
