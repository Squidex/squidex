// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Lucene.Net.Store;
using MongoDB.Driver.GridFS;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoIndexInput : IndexInput
    {
        private readonly IndexInput cacheInput;
        private readonly MongoDirectory indexDirectory;
        private readonly IOContext context;
        private readonly string indexName;

        public override long Length
        {
            get { return cacheInput.Length; }
        }

        public MongoIndexInput(MongoDirectory indexDirectory, IOContext context, string indexName)
            : base(indexDirectory.GetFullName(indexName))
        {
            this.indexDirectory = indexDirectory;
            this.indexName = indexName;

            this.context = context;

            try
            {
                var file = indexDirectory.FindFile(indexName);

                if (file != null)
                {
                    var fileInfo = new FileInfo(indexDirectory.GetFullPath(indexName));

                    var writtenTime = file.Metadata["WrittenTime"].ToUniversalTime();

                    if (!fileInfo.Exists || fileInfo.LastWriteTimeUtc < writtenTime)
                    {
                        using (var fs = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                        {
                            var fullName = indexDirectory.GetFullName(indexName);

                            indexDirectory.Bucket.DownloadToStream(fullName, fs);
                        }
                    }
                }
            }
            catch (GridFSFileNotFoundException)
            {
                throw new FileNotFoundException();
            }

            cacheInput = indexDirectory.CacheDirectory.OpenInput(indexName, context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheInput.Dispose();
            }
        }

        public override long GetFilePointer()
        {
            return cacheInput.GetFilePointer();
        }

        public override byte ReadByte()
        {
            return cacheInput.ReadByte();
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            cacheInput.ReadBytes(b, offset, len);
        }

        public override void Seek(long pos)
        {
            cacheInput.Seek(pos);
        }

        public override object Clone()
        {
            return new MongoIndexInput(indexDirectory, context, indexName);
        }
    }
}
