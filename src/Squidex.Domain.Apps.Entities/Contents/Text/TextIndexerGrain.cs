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
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexerGrain : GrainOfGuid, ITextIndexerGrain
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 2000;
        private const int MaxUpdates = 100;
        private static readonly HashSet<string> IdFields = new HashSet<string>();
        private static readonly Analyzer Analyzer = new MultiLanguageAnalyzer(Version);
        private readonly IAssetStore assetStore;
        private DirectoryInfo directory;
        private IndexWriter indexWriter;
        private IndexReader indexReader;
        private QueryParser queryParser;
        private long currentAppVersion;
        private long currentSchemaVersion;
        private long updates;

        public TextIndexerGrain(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public override Task OnActivateAsync()
        {
            RegisterTimer(_ => FlushAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await DeactivateAsync(true);
        }

        protected override async Task OnActivateAsync(Guid key)
        {
            directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"Index_{key}"));

            await assetStore.DownloadAsync(directory);

            indexWriter = new IndexWriter(FSDirectory.Open(directory), new IndexWriterConfig(Version, Analyzer));
            indexReader = indexWriter.GetReader(true);
        }

        public Task DeleteAsync(Guid id)
        {
            indexWriter.DeleteDocuments(new Term("id", id.ToString()));

            return TryFlushAsync();
        }

        public Task IndexAsync(Guid id, J<IndexData> data)
        {
            string idString = id.ToString(), draft = data.Value.IsDraft.ToString();

            indexWriter.DeleteDocuments(
                new Term("id", idString),
                new Term("dd", draft));

            var document = new Document();

            document.AddStringField("id", idString, Field.Store.YES);
            document.AddStringField("dd", draft, Field.Store.YES);

            var languages = new Dictionary<string, StringBuilder>();

            void AppendText(string language, string text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var sb = languages.GetOrAddNew(language);

                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }

                    sb.Append(text);
                }
            }

            foreach (var field in data.Value.Data)
            {
                foreach (var fieldValue in field.Value)
                {
                    var appendText = new Action<string>(text => AppendText(fieldValue.Key, text));

                    AppendJsonText(fieldValue.Value, appendText);
                }
            }

            foreach (var field in languages)
            {
                document.AddTextField(field.Key, field.Value.ToString(), Field.Store.NO);
            }

            indexWriter.AddDocument(document);

            return TryFlushAsync();
        }

        private static void AppendJsonText(IJsonValue value, Action<string> appendText)
        {
            if (value.Type == JsonValueType.String)
            {
                var text = value.ToString();

                appendText(text);
            }
            else if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    AppendJsonText(item, appendText);
                }
            }
            else if (value is JsonObject obj)
            {
                foreach (var item in obj.Values)
                {
                    AppendJsonText(item, appendText);
                }
            }
        }

        public Task<List<Guid>> SearchAsync(string queryText, J<SearchContext> context)
        {
            var result = new HashSet<Guid>();

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var query = BuildQuery(queryText, context);

                if (indexReader != null)
                {
                    var filter = new QueryWrapperFilter(new TermQuery(new Term("dd", context.Value.IsDraft.ToString())));

                    var hits = new IndexSearcher(indexReader).Search(query, MaxResults).ScoreDocs;

                    foreach (var hit in hits)
                    {
                        var document = indexReader.Document(hit.Doc);

                        var idField = document.GetField("id")?.GetStringValue();

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
            if (queryParser == null || currentAppVersion != context.AppVersion || currentSchemaVersion != context.SchemaVersion)
            {
                var fields = context.AppLanguages.Select(BuildFieldName).ToArray();

                queryParser = new MultiFieldQueryParser(Version, fields, Analyzer);

                currentAppVersion = context.AppVersion;
                currentSchemaVersion = context.SchemaVersion;
            }

            return queryParser.Parse(query);
        }

        private async Task TryFlushAsync()
        {
            updates++;

            if (updates >= MaxUpdates)
            {
                await FlushAsync();
            }
        }

        public async Task FlushAsync()
        {
            if (updates > 0 && indexWriter != null)
            {
                indexWriter.Flush(true, true);
                indexWriter.Commit();

                indexReader?.Dispose();
                indexReader = indexWriter.GetReader(true);

                await assetStore.UploadDirectoryAsync(directory);

                updates = 0;
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

        private static string BuildFieldName(string language)
        {
            return $"field_{language}";
        }
    }
}
