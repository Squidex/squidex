// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class IndexHolder : DisposableObjectBase
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private static readonly MergeScheduler MergeScheduler = new ConcurrentMergeScheduler();
        private static readonly Analyzer SharedAnalyzer = new MultiLanguageAnalyzer(Version);
        private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        private readonly Directory directory;
        private IndexWriter indexWriter;
        private IndexSearcher? indexSearcher;
        private DirectoryReader? indexReader;

        public Analyzer Analyzer
        {
            get
            {
                ThrowIfDisposed();

                return SharedAnalyzer;
            }
        }

        public SnapshotDeletionPolicy Snapshotter
        {
            get
            {
                ThrowIfDisposed();

                return snapshotter;
            }
        }

        public IndexWriter Writer
        {
            get
            {
                ThrowIfDisposed();

                return indexWriter;
            }
        }

        public IndexReader? Reader
        {
            get
            {
                ThrowIfDisposed();

                return indexReader;
            }
        }

        public IndexSearcher? Searcher
        {
            get
            {
                ThrowIfDisposed();

                return indexSearcher;
            }
        }

        public IndexHolder(IDirectoryFactory directoryFactory, Guid schemaId)
        {
            directory = directoryFactory.Create(schemaId);
        }

        public void Open()
        {
            RecreateIndexWriter();

            if (indexWriter.NumDocs > 0)
            {
                EnsureReader();
            }
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                indexWriter.Dispose();
            }
        }

        private void RecreateIndexWriter()
        {
            var config = new IndexWriterConfig(Version, Analyzer)
            {
                IndexDeletionPolicy = snapshotter,
                MergePolicy = new TieredMergePolicy(),
                MergeScheduler = MergeScheduler
            };

            indexWriter = new IndexWriter(directory, config);

            MarkStale();
        }

        public void EnsureReader()
        {
            ThrowIfDisposed();

            if (indexReader == null)
            {
                indexReader = indexWriter.GetReader(true);
                indexSearcher = new IndexSearcher(indexReader);
            }
        }

        public void MarkStale()
        {
            ThrowIfDisposed();

            if (indexReader != null)
            {
                indexReader.Dispose();
                indexReader = null;
                indexSearcher = null;
            }
        }

        public void Commit()
        {
            ThrowIfDisposed();

            try
            {
                MarkStale();

                indexWriter.Commit();
            }
            catch (OutOfMemoryException)
            {
                RecreateIndexWriter();

                throw;
            }
        }
    }
}
