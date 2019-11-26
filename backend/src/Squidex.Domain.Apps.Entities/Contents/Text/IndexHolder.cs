// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class IndexHolder
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private static readonly MergeScheduler MergeScheduler = new ConcurrentMergeScheduler();
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        private readonly DirectoryInfo directory;
        private DirectoryReader indexReader;
        private IndexWriter indexWriter;
        private IndexSearcher indexSearcher;

        public SnapshotDeletionPolicy Snapshotter
        {
            get { return snapshotter; }
        }

        public IndexWriter Writer
        {
            get { return indexWriter; }
        }

        public IndexReader Reader
        {
            get { return indexReader; }
        }

        public IndexSearcher Searcher
        {
            get { return indexSearcher; }
        }

        public IndexHolder(DirectoryInfo directory)
        {
            this.directory = directory;

            RecreateIndexWriter();
        }

        private void RecreateIndexWriter()
        {
            var config = new IndexWriterConfig(Version, Analyzer)
            {
                IndexDeletionPolicy = snapshotter,
                MergePolicy = new TieredMergePolicy(),
                MergeScheduler = MergeScheduler
            };

            indexWriter = new IndexWriter(FSDirectory.Open(directory), config);

            RecreateReader();
        }

        public void RecreateReader()
        {
            if (indexReader != null)
            {
                var newReader = DirectoryReader.OpenIfChanged(indexReader);

                if (newReader != null)
                {
                    indexReader.Dispose();
                    indexReader = newReader;
                    indexSearcher = new IndexSearcher(indexReader);
                }
            }
            else
            {
                indexReader?.Dispose();
                indexReader = indexWriter.GetReader(true);
                indexSearcher = new IndexSearcher(indexReader);
            }
        }

        public void Commit(bool recreate)
        {
            try
            {
                indexReader?.Dispose();
                indexReader = null!;

                indexWriter?.Commit();
                indexWriter?.Dispose(true);
                indexWriter = null!;
            }
            catch (OutOfMemoryException)
            {
                if (recreate)
                {
                    RecreateIndexWriter();
                }

                throw;
            }

            if (recreate)
            {
                RecreateIndexWriter();
            }
        }
    }
}
