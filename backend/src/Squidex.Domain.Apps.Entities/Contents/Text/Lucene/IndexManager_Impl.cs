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
            private readonly Directory directory;
            private IndexWriter indexWriter;
            private IndexSearcher? indexSearcher;
            private DirectoryReader? indexReader;
            private bool isDisposed;

            public Analyzer Analyzer
            {
                get { return SharedAnalyzer; }
            }

            public SnapshotDeletionPolicy Snapshotter
            {
                get { return snapshotter; }
            }

            public Directory Directory
            {
                get { return directory; }
            }

            public IndexWriter Writer
            {
                get
                {
                    ThrowIfReleased();

                    if (indexWriter == null)
                    {
                        throw new InvalidOperationException("Index writer has not been created yet. Call Open()");
                    }

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

            public IndexHolder(Guid id, Directory directory)
            {
                Id = id;

                this.directory = directory;

                RecreateIndexWriter();

                if (indexWriter.NumDocs > 0)
                {
                    EnsureReader();
                }
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    indexReader?.Dispose();
                    indexReader = null;

                    indexWriter.Dispose();

                    isDisposed = true;
                }
            }

            private IndexWriter RecreateIndexWriter()
            {
                var config = new IndexWriterConfig(Version, Analyzer)
                {
                    IndexDeletionPolicy = snapshotter,
                    MergePolicy = new TieredMergePolicy(),
                    MergeScheduler = MergeScheduler
                };

                indexWriter = new IndexWriter(directory, config);

                MarkStale();

                return indexWriter;
            }

            public void EnsureReader()
            {
                ThrowIfReleased();

                if (indexReader == null && indexWriter != null)
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
                if (indexWriter == null)
                {
                    throw new InvalidOperationException("Index is already released or not open yet.");
                }
            }
        }
    }
}
