// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexerGrain : GrainOfGuid
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
        private int currentAppVersion;
        private int currentSchemaVersion;
        private int updates;

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

        public Task DeleteContentAsync(Guid id)
        {
            indexWriter.DeleteDocuments(new Term("id", id.ToString()));

            return TryFlushAsync();
        }

        public Task AddContentAsync(Guid id, NamedContentData data, bool isUpdate, bool isDraft)
        {
            var idString = id.ToString();

            if (isUpdate)
            {
                indexWriter.DeleteDocuments(new Term("id", idString));
            }

            var document = new Document();

            document.AddStringField("id", idString, Field.Store.YES);
            document.AddInt32Field("draft", isDraft ? 1 : 0, Field.Store.YES);

            foreach (var field in data)
            {
                foreach (var fieldValue in field.Value)
                {
                    var value = fieldValue.Value;

                    if (value.Type == JsonValueType.String)
                    {
                        var fieldName = BuildFieldName(fieldValue.Key, field.Key);

                        document.AddTextField(fieldName, fieldValue.Value.ToString(), Field.Store.YES);
                    }
                    else if (value.Type == JsonValueType.Object)
                    {
                        foreach (var property in (JsonObject)value)
                        {
                            if (property.Value.Type == JsonValueType.String)
                            {
                                var fieldName = BuildFieldName(fieldValue.Key, field.Key, property.Key);

                                document.AddTextField(fieldName, property.Value.ToString(), Field.Store.YES);
                            }
                        }
                    }
                }
            }

            indexWriter.AddDocument(document);

            return TryFlushAsync();
        }

        public Task<List<Guid>> SearchAsync(string term, int appVersion, int schemaVersion, J<Schema> schema, List<string> languages)
        {
            var query = BuildQuery(term, appVersion, schemaVersion, schema, languages);

            var result = new List<Guid>();

            if (indexReader != null)
            {
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

            return Task.FromResult(result);
        }

        private Query BuildQuery(string query, int appVersion, int schemaVersion, J<Schema> schema, List<string> language)
        {
            if (queryParser == null || currentAppVersion != appVersion || currentSchemaVersion != schemaVersion)
            {
                var fields = BuildFields(schema, language);

                queryParser = new MultiFieldQueryParser(Version, fields, Analyzer);

                currentAppVersion = appVersion;
                currentSchemaVersion = schemaVersion;
            }

            return queryParser.Parse(query);
        }

        private string[] BuildFields(Schema schema, IEnumerable<string> languages)
        {
            var fieldNames = new List<string>();

            var iv = InvariantPartitioning.Instance.Master.Key;

            foreach (var field in schema.Fields)
            {
                if (field.RawProperties is StringFieldProperties)
                {
                    if (field.Partitioning.Equals(Partitioning.Invariant))
                    {
                        fieldNames.Add(BuildFieldName(iv, field.Name));
                    }
                    else
                    {
                        foreach (var language in languages)
                        {
                            fieldNames.Add(BuildFieldName(language, field.Name));
                        }
                    }
                }
                else if (field is IArrayField arrayField)
                {
                    foreach (var nested in arrayField.Fields)
                    {
                        if (nested.RawProperties is StringFieldProperties)
                        {
                            if (field.Partitioning.Equals(Partitioning.Invariant))
                            {
                                fieldNames.Add(BuildFieldName(iv, field.Name, nested.Name));
                            }
                            else
                            {
                                foreach (var language in languages)
                                {
                                    fieldNames.Add(BuildFieldName(language, field.Name, nested.Name));
                                }
                            }
                        }
                    }
                }
            }

            return fieldNames.ToArray();
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

        private static string BuildFieldName(string language, string name)
        {
            return $"{language}_{name}";
        }

        private static string BuildFieldName(string language, string name, string nested)
        {
            return $"{language}_{name}_{nested}";
        }
    }
}
