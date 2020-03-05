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

            SetLockFactory(cacheDirectory.LockFactory);
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
            try
            {
                var file = FindFile(name);

                if (file != null)
                {
                    using (var fs = new FileStream(GetFullPath(name), FileMode.Create, FileAccess.Write))
                    {
                        Bucket.DownloadToStream(file.Id, fs);
                    }
                }
            }
            catch (GridFSFileNotFoundException)
            {
                throw new FileNotFoundException();
            }

            return cacheDirectory.CreateOutput(name, context);
        }

        public override IndexInput OpenInput(string name, IOContext context)
        {
            return cacheDirectory.OpenInput(name, context);
        }

        public override void DeleteFile(string name)
        {
            EnsureNotDisposed();

            try
            {
                Bucket.Delete(GetFileId(name));
            }
            catch (GridFSFileNotFoundException)
            {
            }

            cacheDirectory.DeleteFile(name);
        }

        public override long FileLength(string name)
        {
            EnsureNotDisposed();

            try
            {
                return cacheDirectory.FileLength(name);
            }
            catch (FileNotFoundException)
            {
                var file = FindFile(name);

                if (file == null)
                {
                     throw new FileNotFoundException(null, GetFileId(name));
                }

                return file.Length;
            }
        }

        public override string[] ListAll()
        {
            EnsureNotDisposed();

            var files = new HashSet<string>(cacheDirectory.ListAll());

            try
            {
                var mongoFiles = Bucket.Find(CreateFilter()).ToList();

                foreach (var file in mongoFiles)
                {
                    files.Add(file.Filename);
                }
            }
            catch (Exception ex)
            {
                if (files.Count == 0)
                {
                    throw ex;
                }
            }

            return files.ToArray();
        }

        public GridFSFileInfo<string>? FindFile(string name)
        {
            var fullName = GetFileId(name);

            return Bucket.Find(Builders<GridFSFileInfo<string>>.Filter.Eq(x => x.Id, fullName)).FirstOrDefault();
        }

        public override void Sync(ICollection<string> names)
        {
            EnsureNotDisposed();

            foreach (var name in names)
            {
                var file = new FileInfo(GetFullPath(name));

                if (file.Exists)
                {
                    using (var fs = file.OpenRead())
                    {
                        var fullName = GetFileId(name);

                        try
                        {
                            Bucket.UploadFromStream(fullName, name, fs);
                        }
                        catch (MongoBulkWriteException ex) when (ex.WriteErrors.Any(x => x.Category == ServerErrorCategory.DuplicateKey))
                        {
                            Bucket.Delete(fullName);
                            Bucket.UploadFromStream(fullName, name, fs);
                        }
                        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                        {
                            Bucket.Delete(fullName);
                            Bucket.UploadFromStream(fullName, name, fs);
                        }
                    }
                }
            }
        }

        public string GetFileId(string name)
        {
            return $"{directory}/{name}";
        }

        public string GetFullPath(string name)
        {
            return Path.Combine(cacheDirectoryInfo.FullName, name);
        }

        private FilterDefinition<GridFSFileInfo<string>> CreateFilter()
        {
            return Builders<GridFSFileInfo<string>>.Filter.Regex(x => x.Id, new BsonRegularExpression($"^{directory}/"));
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        [Obsolete]
        public override bool FileExists(string name)
        {
            throw new NotSupportedException();
        }
    }
}
