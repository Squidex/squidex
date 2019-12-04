// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexerGrain : GrainOfGuid, ITextIndexerGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 400;
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly string[] Invariant = { InvariantPartitioning.Key };
        private readonly IndexManager indexManager;
        private IDisposable? timer;
        private IIndex index;
        private IndexState indexState;
        private QueryParser? queryParser;
        private HashSet<string>? currentLanguages;
        private int updates;

        public TextIndexerGrain(IndexManager indexManager)
        {
            Guard.NotNull(indexManager);

            this.indexManager = indexManager;
        }

        public override async Task OnDeactivateAsync()
        {
            if (index != null)
            {
                await indexManager.ReleaseAsync(index);
            }
        }

        protected override async Task OnActivateAsync(Guid key)
        {
            index = await indexManager.AcquireAsync(key);

            indexState = new IndexState(index);
        }

        public Task<bool> IndexAsync(Update update)
        {
            var content = new TextIndexContent(index, indexState, update.Id);

            content.Index(update.Text, update.OnlyDraft);

            return TryCommitAsync();
        }

        public Task<bool> CopyAsync(Guid id, bool fromDraft)
        {
            var content = new TextIndexContent(index, indexState, id);

            content.Copy(fromDraft);

            return TryCommitAsync();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var content = new TextIndexContent(index, indexState, id);

            content.Delete();

            return TryCommitAsync();
        }

        public Task<List<Guid>> SearchAsync(string queryText, SearchContext context)
        {
            var result = new List<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                index.EnsureReader();

                if (index.Searcher != null)
                {
                    var query = BuildQuery(queryText, context);

                    var hits = index.Searcher.Search(query, MaxResults).ScoreDocs;

                    if (hits.Length > 0)
                    {
                        var found = new HashSet<Guid>();

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
                }
            }

            return Task.FromResult(result);
        }

        private Query BuildQuery(string query, SearchContext context)
        {
            if (queryParser == null || currentLanguages == null || !currentLanguages.SetEquals(context.Languages))
            {
                var fields = context.Languages.Union(Invariant).ToArray();

                queryParser = new MultiFieldQueryParser(Version, fields, index.Analyzer);

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

        private async Task<bool> TryCommitAsync()
        {
            timer?.Dispose();

            updates++;

            if (updates >= MaxUpdates)
            {
                await CommitAsync();

                return true;
            }
            else
            {
                index.MarkStale();

                try
                {
                    timer = RegisterTimer(_ => CommitAsync(), null, CommitDelay, CommitDelay);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task CommitAsync()
        {
            if (updates > 0)
            {
                await indexManager.CommitAsync(index);

                updates = 0;
            }
        }
    }
}
