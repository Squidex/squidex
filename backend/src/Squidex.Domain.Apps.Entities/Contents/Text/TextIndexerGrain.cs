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
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexerGrain : GrainOfGuid, ITextIndexerGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 400;
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private static readonly string[] Invariant = { InvariantPartitioning.Key };
        private readonly IDirectoryFactory directoryFactory;
        private IDisposable? timer;
        private IndexHolder index;
        private IndexState indexState;
        private QueryParser? queryParser;
        private HashSet<string>? currentLanguages;
        private int updates;

        public TextIndexerGrain(IDirectoryFactory directoryFactory)
        {
            Guard.NotNull(directoryFactory);

            this.directoryFactory = directoryFactory;
        }

        public override async Task OnDeactivateAsync()
        {
            await DeactivateAsync(true);
        }

        protected override Task OnActivateAsync(Guid key)
        {
            index = new IndexHolder(directoryFactory, key);
            indexState = new IndexState(index);

            return TaskHelper.Done;
        }

        public Task<bool> IndexAsync(J<Update> update)
        {
            return IndexInternalAsync(update);
        }

        private Task<bool> IndexInternalAsync(Update update)
        {
            var content = new TextIndexContent(index, indexState, update.Id);

            content.Index(update.Data, update.OnlyDraft);

            return TryFlushAsync();
        }

        public Task<bool> CopyAsync(Guid id, bool fromDraft)
        {
            var content = new TextIndexContent(index, indexState, id);

            content.Copy(fromDraft);

            return TryFlushAsync();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var content = new TextIndexContent(index, indexState, id);

            content.Delete();

            return TryFlushAsync();
        }

        public Task<List<Guid>> SearchAsync(string queryText, SearchContext context)
        {
            var result = new List<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var query = BuildQuery(queryText, context);

                var found = new HashSet<Guid>();

                var hits = index.Searcher.Search(query, MaxResults).ScoreDocs;

                foreach (var hit in hits)
                {
                    if (TextIndexContent.TryGetId(hit.Doc, context.Scope, index, indexState, out var id))
                    {
                        if (found.Add(id))
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
            if (queryParser == null || currentLanguages == null || !currentLanguages.SetEquals(context.Languages))
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
                await FlushAsync(true);

                return true;
            }
            else
            {
                index.RecreateReader();

                try
                {
                    timer = RegisterTimer(_ => FlushAsync(true), null, CommitDelay, CommitDelay);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return false;
        }

        public Task FlushAsync(bool recreate)
        {
            if (updates > 0)
            {
                index.Commit(recreate);

                updates = 0;
            }

            return TaskHelper.Done;
        }

        public async Task DeactivateAsync(bool deleteFolder = false)
        {
            if (updates > 0)
            {
                await FlushAsync(false);
            }
            else
            {
                index?.Commit(false);
            }

            index?.Dispose();
        }
    }
}
