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
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexerGrain : GrainOfGuid, ITextIndexerGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 400;
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly MergeScheduler MergeScheduler = new ConcurrentMergeScheduler();
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private static readonly string[] Invariant = { InvariantPartitioning.Instance.Master.Key };
        private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        private readonly IAssetStore assetStore;
        private IDisposable timer;
        private DirectoryInfo directory;
        private IndexWriter indexWriter;
        private IndexReader indexReader;
        private IndexSearcher indexSearcher;
        private IndexState indexState;
        private QueryParser queryParser;
        private HashSet<string> currentLanguages;
        private int updates;

        public TextIndexerGrain(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public override async Task OnDeactivateAsync()
        {
            await DeactivateAsync(true);
        }

        protected override async Task OnActivateAsync(Guid key)
        {
            directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"Index_{key}"));

            await assetStore.DownloadAsync(directory);

            var config = new IndexWriterConfig(Version, Analyzer)
            {
                IndexDeletionPolicy = snapshotter,
                MergePolicy = new TieredMergePolicy(),
                MergeScheduler = MergeScheduler
            };

            indexWriter = new IndexWriter(FSDirectory.Open(directory), config);

            if (indexWriter.NumDocs > 0)
            {
                OpenReader();
            }
            else
            {
                indexState = new IndexState(indexWriter);
            }
        }

        public Task<bool> IndexAsync(J<Update> update)
        {
            return IndexInternalAsync(update);
        }

        private Task<bool> IndexInternalAsync(Update update)
        {
            var content = new TextIndexContent(indexWriter, indexState, update.Id);

            content.Index(update.Data, update.OnlyDraft);

            return TryFlushAsync();
        }

        public Task<bool> CopyAsync(Guid id, bool fromDraft)
        {
            var content = new TextIndexContent(indexWriter, indexState, id);

            content.Copy(fromDraft);

            return TryFlushAsync();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var content = new TextIndexContent(indexWriter, indexState, id);

            content.Delete();

            return TryFlushAsync();
        }

        public Task<List<Guid>> SearchAsync(string queryText, SearchContext context)
        {
            var result = new List<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var query = BuildQuery(queryText, context);

                if (indexReader == null && indexWriter.NumDocs > 0)
                {
                    OpenReader();
                }

                if (indexReader != null)
                {
                    var found = new HashSet<Guid>();

                    var hits = indexSearcher.Search(query, MaxResults).ScoreDocs;

                    foreach (var hit in hits)
                    {
                        if (TextIndexContent.TryGetId(hit.Doc, context.Scope, indexReader, indexState, out var id))
                        {
                            if (found.Add(id))
                            {
                                result.Add(id);
                            }
                        }
                    }
                }
            }

            return Task.FromResult(result.ToList());
        }

        private Query BuildQuery(string query, SearchContext context)
        {
            if (queryParser == null || !currentLanguages.SetEquals(context.Languages))
            {
                var fields = context.Languages.Union(Invariant).ToArray();

                queryParser = new MultiFieldQueryParser(Version, fields, Analyzer);

                currentLanguages = context.Languages;
            }

            try
            {
                return queryParser.Parse(query);
            }
            catch (ParseException ex)
            {
                throw new ValidationException(ex.Message);
            }
        }

        private async Task<bool> TryFlushAsync()
        {
            timer?.Dispose();

            updates++;

            if (updates >= MaxUpdates)
            {
                await FlushAsync();

                return true;
            }
            else
            {
                CleanReader();

                try
                {
                    timer = RegisterTimer(_ => FlushAsync(), null, CommitDelay, CommitDelay);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task FlushAsync()
        {
            if (updates > 0 && indexWriter != null)
            {
                indexWriter.Commit();
                indexWriter.Flush(true, true);

                CleanReader();

                var commit = snapshotter.Snapshot();
                try
                {
                    await assetStore.UploadDirectoryAsync(directory, commit);
                }
                finally
                {
                    snapshotter.Release(commit);
                }

                updates = 0;
            }
        }

        public async Task DeactivateAsync(bool deleteFolder = false)
        {
            await FlushAsync();

            CleanWriter();
            CleanReader();

            if (deleteFolder && directory.Exists)
            {
                directory.Delete(true);
            }
        }

        private void OpenReader()
        {
            indexReader = indexWriter.GetReader(true);
            indexSearcher = new IndexSearcher(indexReader);
            indexState = new IndexState(indexWriter, indexReader, indexSearcher);
        }

        private void CleanReader()
        {
            indexReader?.Dispose();
            indexReader = null;
            indexSearcher = null;
            indexState?.CloseReader();
        }

        private void CleanWriter()
        {
            indexWriter?.Dispose();
            indexWriter = null;
        }
    }
}
