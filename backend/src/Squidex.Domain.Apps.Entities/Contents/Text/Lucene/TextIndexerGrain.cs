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
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public sealed class TextIndexerGrain : GrainOfGuid, ITextIndexerGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 400;
        private const string MetaId = "_id";
        private const string MetaFor = "_fd";
        private const string MetaContentId = "_cid";
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly string[] Invariant = { InvariantPartitioning.Key };
        private readonly IndexManager indexManager;
        private IDisposable? timer;
        private IIndex index;
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
                        var buffer = new BytesRef(2);

                        var found = new HashSet<Guid>();

                        foreach (var hit in hits)
                        {
                            var forValue = index.Reader.GetBinaryValue(MetaFor, hit.Doc, buffer);

                            if (context.Scope == SearchScope.All && forValue.Bytes[0] != 1)
                            {
                                continue;
                            }

                            if (context.Scope == SearchScope.Published && forValue.Bytes[1] != 1)
                            {
                                continue;
                            }

                            var document = index.Searcher.Doc(hit.Doc);

                            if (document != null)
                            {
                                var idString = document.Get(MetaContentId);

                                if (Guid.TryParse(idString, out var id))
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

        public Task DeleteAsync(Guid id)
        {
            index.Writer.DeleteDocuments(new Term(MetaContentId, id.ToString()));

            return TryCommitAsync();
        }

        public Task UpdateAsync(Immutable<UpdateIndexEntry[]> updates)
        {
            foreach (var update in updates.Value)
            {
                index.Writer.UpdateBinaryDocValue(new Term(MetaId, update.DocId), MetaFor, GetValue(update.ServeAll, update.ServePublished));
            }

            return TryCommitAsync();
        }

        public Task IndexAsync(Immutable<IIndexCommand[]> updates)
        {
            foreach (var command in updates.Value)
            {
                switch (command)
                {
                    case DeleteIndexEntry delete:
                        index.Writer.DeleteDocuments(new Term(MetaId, delete.DocId));
                        break;
                    case UpdateIndexEntry update:
                        index.Writer.UpdateBinaryDocValue(new Term(MetaId, update.DocId), MetaFor, GetValue(update.ServeAll, update.ServePublished));
                        break;
                    case UpsertIndexEntry upsert:
                        {
                            var document = new Document();

                            document.SetField(MetaId, upsert.DocId);
                            document.SetField(MetaContentId, upsert.ContentId.ToString());
                            document.SetBinaryDocValue(MetaFor, GetValue(upsert.ServeAll, upsert.ServePublished));

                            foreach (var (key, value) in upsert.Texts)
                            {
                                document.AddTextField(key, value, Field.Store.NO);
                            }

                            index.Writer.UpdateDocument(new Term(MetaId, upsert.DocId), document);

                            break;
                        }
                }
            }

            return TryCommitAsync();
        }

        private static BytesRef GetValue(bool forDraft, bool forPublished)
        {
            return new BytesRef(new[]
            {
                (byte)(forDraft ? 1 : 0),
                (byte)(forPublished ? 1 : 0)
            });
        }
    }
}
