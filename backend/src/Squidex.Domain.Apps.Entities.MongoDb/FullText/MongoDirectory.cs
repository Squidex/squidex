// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Store;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoDirectory : BaseDirectory
    {
        private readonly IGridFSBucket<string> bucket;
        private readonly string directory;
        private readonly DirectoryInfo cacheDirectoryInfo;
        private readonly LuceneDirectory cacheDirectory;
        private bool isDisposed;

        public LuceneDirectory CacheDirectory
        {
            get { return cacheDirectory; }
        }

        public DirectoryInfo CacheDirectoryInfo
        {
            get { return cacheDirectoryInfo; }
        }

        public IGridFSBucket<string> Bucket
        {
            get { return bucket; }
        }

        public MongoDirectory(IGridFSBucket<string> bucket, string directory, DirectoryInfo cacheDirectoryInfo)
        {
            this.bucket = bucket;

            this.directory = directory;

            this.cacheDirectoryInfo = cacheDirectoryInfo;

            cacheDirectoryInfo.Create();
            cacheDirectory = FSDirectory.Open(cacheDirectoryInfo);

            SetLockFactory(new NativeFSLockFactory(cacheDirectoryInfo));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                isDisposed = true;

                cacheDirectory.Dispose();
            }
        }

        public override string GetLockID()
        {
            return cacheDirectory.GetLockID();
        }

        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            return new MongoIndexOutput(this, context, name);
        }

        public override IndexInput OpenInput(string name, IOContext context)
        {
            return new MongoIndexInput(this, context, name);
        }

        public override void DeleteFile(string name)
        {
            EnsureNotDisposed();

            var fullName = GetFullName(name);

            try
            {
                Bucket.Delete(fullName);
            }
            catch (GridFSFileNotFoundException)
            {
            }
        }

        public override long FileLength(string name)
        {
            EnsureNotDisposed();

            var file = FindFile(name) ?? throw new FileNotFoundException();

            return file.Length;
        }

        public override string[] ListAll()
        {
            EnsureNotDisposed();

            var files = Bucket.Find(Builders<GridFSFileInfo<string>>.Filter.Regex(x => x.Id, new BsonRegularExpression($"^{directory}/"))).ToList();

            return files.Select(x => x.Filename).ToArray();
        }

        public GridFSFileInfo<string>? FindFile(string name)
        {
            var fullName = GetFullName(name);

            return Bucket.Find(Builders<GridFSFileInfo<string>>.Filter.Eq(x => x.Id, fullName)).FirstOrDefault();
        }

        public override void Sync(ICollection<string> names)
        {
        }

        [Obsolete]
        public override bool FileExists(string name)
        {
            throw new NotSupportedException();
        }

        public string GetFullName(string name)
        {
            return $"{directory}/{name}";
        }

        public string GetFullPath(string name)
        {
            return Path.Combine(cacheDirectoryInfo.FullName, name);
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
