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

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public sealed partial class IndexManager
    {
        private sealed class IndexHolder : IDisposable, IIndex
        {
            private const LuceneVersion Version = LuceneVersion.LUCENE_48;
            private static readonly MergeScheduler MergeScheduler = new ConcurrentMergeScheduler();
            private static readonly Analyzer SharedAnalyzer = new MultiLanguageAnalyzer(Version);
            private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
            private Directory directory;
            private IndexWriter indexWriter;
            private IndexSearcher? indexSearcher;
            private DirectoryReader? indexReader;
            private bool isReleased;

            public Analyzer Analyzer
            {
                get { return SharedAnalyzer; }
            }

            public SnapshotDeletionPolicy Snapshotter
            {
                get { return snapshotter; }
            }

            public IndexWriter Writer
            {
                get
                {
                    ThrowIfReleased();

                    return indexWriter;
                }
            }

            public IndexReader? Reader
            {
                get
                {
                    ThrowIfReleased();

                    return indexReader;
                }
            }

            public IndexSearcher? Searcher
            {
                get
                {
                    ThrowIfReleased();

                    return indexSearcher;
                }
            }

            public Guid Id { get; }

            public IndexHolder(Guid id)
            {
                Id = id;
            }

            public void Dispose()
            {
                indexReader?.Dispose();
                indexWriter?.Dispose();
            }

            public void Open(Directory directory)
            {
                Guard.NotNull(directory);

                this.directory = directory;

                RecreateIndexWriter();

                if (indexWriter.NumDocs > 0)
                {
                    EnsureReader();
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
                ThrowIfReleased();

                if (indexReader == null)
                {
                    indexReader = indexWriter.GetReader(true);
                    indexSearcher = new IndexSearcher(indexReader);
                }
            }

            public void MarkStale()
            {
                ThrowIfReleased();

                MarkStaleInternal();
            }

            private void MarkStaleInternal()
            {
                if (indexReader != null)
                {
                    indexReader.Dispose();
                    indexReader = null;
                    indexSearcher = null;
                }
            }

            internal void Commit()
            {
                try
                {
                    MarkStaleInternal();

                    indexWriter.Commit();
                }
                catch (OutOfMemoryException)
                {
                    RecreateIndexWriter();

                    throw;
                }
            }

            private void ThrowIfReleased()
            {
                if (isReleased)
                {
                    throw new InvalidOperationException("Index is already released.");
                }
            }

            internal void Release()
            {
                isReleased = true;
            }

            internal IndexWriter GetUnsafeWriter()
            {
                return indexWriter;
            }
        }
    }
}
