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
        private const int MaxUpdates = 100;
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private static readonly string[] Invariant = { InvariantPartitioning.Instance.Master.Key };
        private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        private readonly IAssetStore assetStore;
        private IDisposable timer;
        private DirectoryInfo directory;
        private IndexWriter indexWriter;
        private IndexReader indexReader;
        private IndexSearcher indexSearcher;
        private BinaryDocValues indexValues;
        private QueryParser queryParser;
        private HashSet<string> currentLanguages;
        private long updates;
        private long updatesNotWritten;

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
                IndexDeletionPolicy = snapshotter
            };

            indexWriter = new IndexWriter(FSDirectory.Open(directory), config);

            if (indexWriter.NumDocs > 0)
            {
                indexReader = indexWriter.GetReader(false);
                indexSearcher = new IndexSearcher(indexReader);
                indexValues = TextIndexContent.CreateValues(indexReader);
            }
        }

        public Task DeleteAsync(Guid id)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, indexValues, id);

            content.Delete();

            return TryFlushAsync();
        }

        public Task IndexAsync(Guid id, J<IndexData> data, bool onlyDraft)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, indexValues, id);

            content.Index(data.Value.DataDraft, data.Value.Data, indexValues, onlyDraft);

            return TryFlushAsync();
        }

        public Task CopyAsync(Guid id, bool fromDraft)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, indexValues, id);

            content.Copy(fromDraft);

            return TryFlushAsync();
        }

        public Task<List<Guid>> SearchAsync(string queryText, SearchContext context)
        {
            var result = new HashSet<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var query = BuildQuery(queryText, context);

                if (indexReader != null)
                {
                    var hits = indexSearcher.Search(query, MaxResults).ScoreDocs;

                    foreach (var hit in hits)
                    {
                        if (TextIndexContent.TryGetId(hit.Doc, context.Scope, indexReader, indexValues, out var id))
                        {
                            result.Add(id);
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

        private async Task TryFlushAsync()
        {
            updates++;
            updatesNotWritten++;

            if (updates >= MaxUpdates)
            {
                await FlushAsync(true);
            }
            else
            {
                await FlushAsync();

                timer?.Dispose();

                try
                {
                    timer = RegisterTimer(_ => FlushAsync(true), null, CommitDelay, CommitDelay);
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        public async Task FlushAsync(bool write = false)
        {
            if (updates > 0 && indexWriter != null)
            {
                indexWriter.Commit();
                indexWriter.Flush(true, true);

                indexReader?.Dispose();
                indexReader = indexWriter.GetReader(false);
                indexSearcher = new IndexSearcher(indexReader);
                indexValues = TextIndexContent.CreateValues(indexReader);

                updates = 0;
            }

            if (updatesNotWritten > 0 && write)
            {
                var commit = snapshotter.Snapshot();
                try
                {
                    await assetStore.UploadDirectoryAsync(directory, commit);
                }
                finally
                {
                    snapshotter.Release(commit);
                }

                updatesNotWritten = 0;
            }

            timer?.Dispose();
        }

        public async Task DeactivateAsync(bool deleteFolder = false)
        {
            await FlushAsync(true);

            indexWriter?.Dispose();
            indexWriter = null;

            indexReader?.Dispose();
            indexReader = null;

            if (deleteFolder && directory.Exists)
            {
                directory.Delete(true);
            }
        }
    }
}
