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
using Lucene.Net.Queries;
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
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(30);
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private static readonly TermsFilter DraftFilter = new TermsFilter(new Term(TextIndexContent.MetaDraft, true.ToString()));
        private static readonly TermsFilter NoDraftFilter = new TermsFilter(new Term(TextIndexContent.MetaDraft, false.ToString()));
        private readonly SnapshotDeletionPolicy snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        private readonly IAssetStore assetStore;
        private IDisposable timer;
        private DirectoryInfo directory;
        private IndexWriter indexWriter;
        private IndexReader indexReader;
        private IndexSearcher indexSearcher;
        private QueryParser queryParser;
        private HashSet<string> currentLanguages;
        private long updates;

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
            }
        }

        public Task DeleteAsync(Guid id)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, id);

            content.Delete();

            return TryFlushAsync();
        }

        public Task IndexAsync(Guid id, J<IndexData> data, bool onlyDraft)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, id);

            content.Index(data.Value.Data, onlyDraft);

            return TryFlushAsync();
        }

        public Task CopyAsync(Guid id, bool fromDraft)
        {
            var content = new TextIndexContent(indexWriter, indexSearcher, id);

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
                    var filter = context.IsDraft ? DraftFilter : NoDraftFilter;

                    var hits = indexSearcher.Search(query, filter, MaxResults).ScoreDocs;

                    foreach (var hit in hits)
                    {
                        var document = indexReader.Document(hit.Doc);

                        var idField = document.GetField(TextIndexContent.MetaId)?.GetStringValue();

                        if (idField != null && Guid.TryParse(idField, out var guid))
                        {
                            result.Add(guid);
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
                var fields =
                    context.Languages
                        .Union(Enumerable.Repeat(InvariantPartitioning.Instance.Master.Key, 1)).ToArray();

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

            if (updates >= MaxUpdates)
            {
                await FlushAsync();
            }
            else
            {
                timer?.Dispose();

                try
                {
                    timer = RegisterTimer(_ => FlushAsync(), null, CommitDelay, CommitDelay);
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        public async Task FlushAsync()
        {
            if (updates > 0 && indexWriter != null)
            {
                indexWriter.Commit();
                indexWriter.Flush(true, true);

                indexReader?.Dispose();
                indexReader = indexWriter.GetReader(false);
                indexSearcher = new IndexSearcher(indexReader);

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
            else
            {
                timer?.Dispose();
            }
        }

        public async Task DeactivateAsync(bool deleteFolder = false)
        {
            await TryFlushAsync();

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
