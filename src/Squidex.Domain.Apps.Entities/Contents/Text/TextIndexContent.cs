// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal sealed class TextIndexContent
    {
        private const string MetaId = "_id";
        private const string MetaKey = "_key";
        private const string MetaFor = "_fd";
        private const int ForDraftIndex = 0;
        private const int ForPublishedIndex = 1;
        private readonly IndexWriter indexWriter;
        private readonly IndexSearcher indexSearcher;
        private readonly Guid id;

        public TextIndexContent(IndexWriter indexWriter, IndexSearcher indexSearcher, Guid id)
        {
            this.indexWriter = indexWriter;
            this.indexSearcher = indexSearcher;

            this.id = id;
        }

        public static BinaryDocValues CreateValues(IndexReader indexReader)
        {
            return MultiDocValues.GetBinaryValues(indexReader, MetaFor);
        }

        public static bool TryGetId(int docId, bool forDraft, IndexReader reader, BinaryDocValues values, out Guid result)
        {
            result = Guid.Empty;

            var forValue = new BytesRef();

            values.Get(docId, forValue);

            if (forValue.Bytes.Length != 2)
            {
                return false;
            }

            if (forDraft && forValue.Bytes[ForDraftIndex] != 1)
            {
                return false;
            }

            if (!forDraft && forValue.Bytes[ForPublishedIndex] != 1)
            {
                return false;
            }

            var document = reader.Document(docId);

            var id = document.Get(MetaId);

            if (!Guid.TryParse(id, out result))
            {
                return false;
            }

            return true;
        }

        public void Index(NamedContentData data, NamedContentData dataDraft, bool onlyDraft)
        {
            var converted = CreateDocument(data);

            Upsert(converted, 1, 1, 0);

            var existing = GetDocument(1);

            if (dataDraft != null)
            {
                converted = CreateDocument(dataDraft);

                Upsert(converted, 0, 0, 1);
            }
            else if (IsForPublished(existing))
            {
                Upsert(converted, 0, 0, 1);
            }
            else if (existing == null)
            {
                Upsert(converted, 0, 0, 0);
            }
        }

        private static bool IsForPublished(Document existing)
        {
            return existing?.GetField(MetaFor)?.GetBinaryValue().Bytes[ForPublishedIndex] == 1;
        }

        public void Delete()
        {
            indexWriter.DeleteDocuments(new Term(MetaId, id.ToString()));
        }

        public void Copy(bool fromDraft)
        {
            if (fromDraft)
            {
                Update(1, 0, 0);
                Update(0, 1, 1);
            }
            else
            {
                Update(1, 1, 1);
                Update(0, 0, 0);
            }
        }

        private Document CreateDocument(NamedContentData data)
        {
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

            foreach (var field in data)
            {
                foreach (var fieldValue in field.Value)
                {
                    var appendText = new Action<string>(text => AppendText(fieldValue.Key, text));

                    AppendJsonText(fieldValue.Value, appendText);
                }
            }

            Document document = null;

            if (languages.Count > 0)
            {
                document = new Document();

                foreach (var field in languages)
                {
                    document.AddTextField(field.Key, field.Value.ToString(), Field.Store.NO);
                }
            }

            return document;
        }

        private void Update(byte draft, byte forDraft, byte forPublished)
        {
            var term = new Term(MetaKey, BuildKey(draft));

            indexWriter.UpdateBinaryDocValue(term, MetaFor, GetValue(forDraft, forPublished));
        }

        private Document GetDocument(byte draft)
        {
            if (indexSearcher == null)
            {
                return null;
            }

            var docs = indexSearcher.Search(new TermQuery(new Term(MetaKey, BuildKey(draft))), 1);

            if (docs.ScoreDocs.Length > 0)
            {
                return indexSearcher.Doc(docs.ScoreDocs[0].Doc);
            }

            return null;
        }

        private void Upsert(Document document, byte draft, byte forDraft, byte forPublished)
        {
            if (document != null)
            {
                document.RemoveField(MetaId);
                document.RemoveField(MetaKey);
                document.RemoveField(MetaFor);

                var docId = id.ToString();
                var docKey = BuildKey(draft);

                document.AddStringField(MetaId, docId, Field.Store.YES);
                document.AddStringField(MetaKey, docKey, Field.Store.YES);

                document.AddBinaryDocValuesField(MetaFor, GetValue(forDraft, forPublished));

                indexWriter.DeleteDocuments(new Term(MetaKey, docKey));
                indexWriter.AddDocument(document);
            }
        }

        private static void AppendJsonText(IJsonValue value, Action<string> appendText)
        {
            if (value.Type == JsonValueType.String)
            {
                appendText(value.ToString());
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

        private static BytesRef GetValue(byte forDraft, byte forPublished)
        {
            return new BytesRef(new byte[] { forDraft, forPublished });
        }

        private string BuildKey(byte draft)
        {
            return $"{id}_{draft}";
        }
    }
}
