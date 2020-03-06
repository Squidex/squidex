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
    public sealed class LuceneTextIndexGrain : GrainOfGuid, ILuceneTextIndexGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 400;
        private const string MetaId = "_id";
        private const string MetaFor = "_fd";
        private const string MetaContentId = "_cid";
        private const string MetaSchemaId = "_si";
        private const string MetaSchemaName = "_sn";
        private static readonly TimeSpan CommitDelay = TimeSpan.FromSeconds(10);
        private static readonly string[] Invariant = { InvariantPartitioning.Key };
        private readonly IndexManager indexManager;
        private IDisposable? timer;
        private IIndex index;
        private QueryParser? queryParser;
        private HashSet<string>? currentLanguages;
        private int updates;

        public LuceneTextIndexGrain(IndexManager indexManager)
        {
            Guard.NotNull(indexManager);

            this.indexManager = indexManager;
        }

        public override async Task OnDeactivateAsync()
        {
            if (index != null)
            {
                await CommitAsync();

                await indexManager.ReleaseAsync(index);
            }
        }

        protected override async Task OnActivateAsync(Guid key)
        {
            index = await indexManager.AcquireAsync(key);
        }

        public Task<List<Guid>> SearchAsync(string queryText, SearchFilter? filter, SearchContext context)
        {
            var result = new List<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                index.EnsureReader();

                if (index.Searcher != null)
                {
                    var query = BuildQuery(queryText, filter, context);

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

        private Query BuildQuery(string query, SearchFilter? filter, SearchContext context)
        {
            if (queryParser == null || currentLanguages == null || !currentLanguages.SetEquals(context.Languages))
            {
                var fields = context.Languages.Union(Invariant).ToArray();

                queryParser = new MultiFieldQueryParser(Version, fields, index.Analyzer);

                currentLanguages = context.Languages;
            }

            try
            {
                var byQuery = queryParser.Parse(query);

                if (filter?.SchemaIds.Count > 0)
                {
                    var bySchemas = new BooleanQuery
                    {
                        Boost = 2f
                    };

                    foreach (var schemaId in filter.SchemaIds)
                    {
                        var term = new Term(MetaSchemaId, schemaId.ToString());

                        bySchemas.Add(new TermQuery(term), Occur.SHOULD);
                    }

                    var occur = filter.Must ? Occur.MUST : Occur.SHOULD;

                    return new BooleanQuery
                    {
                        { byQuery, Occur.MUST },
                        { bySchemas, occur }
                    };
                }

                return byQuery;
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

        public Task IndexAsync(NamedId<Guid> schemaId, Immutable<IndexCommand[]> updates)
        {
            foreach (var command in updates.Value)
            {
                switch (command)
                {
                    case DeleteIndexEntry delete:
                        index.Writer.DeleteDocuments(new Term(MetaId, delete.DocId));
                        break;
                    case UpdateIndexEntry update:
                        try
                        {
                            var values = GetValue(update.ServeAll, update.ServePublished);

                            index.Writer.UpdateBinaryDocValue(new Term(MetaId, update.DocId), MetaFor, values);
                        }
                        catch (ArgumentException)
                        {
                        }

                        break;
                    case UpsertIndexEntry upsert:
                        {
                            var document = new Document();

                            document.AddStringField(MetaId, upsert.DocId, Field.Store.YES);
                            document.AddStringField(MetaContentId, upsert.ContentId.ToString(), Field.Store.YES);
                            document.AddStringField(MetaSchemaId, schemaId.Id.ToString(), Field.Store.YES);
                            document.AddStringField(MetaSchemaName, schemaId.Name, Field.Store.YES);
                            document.AddBinaryDocValuesField(MetaFor, GetValue(upsert.ServeAll, upsert.ServePublished));

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
