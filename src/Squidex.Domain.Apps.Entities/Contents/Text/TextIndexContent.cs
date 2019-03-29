// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly BinaryDocValues indexValues;
        private readonly Guid id;

        public TextIndexContent(IndexWriter indexWriter, IndexSearcher indexSearcher, BinaryDocValues indexValues, Guid id)
        {
            this.indexWriter = indexWriter;
            this.indexSearcher = indexSearcher;
            this.indexValues = indexValues;

            this.id = id;
        }

        public static BinaryDocValues CreateValues(IndexReader indexReader)
        {
            return MultiDocValues.GetBinaryValues(indexReader, MetaFor);
        }

        public void Delete()
        {
            indexWriter.DeleteDocuments(new Term(MetaId, id.ToString()));
        }

        public static bool TryGetId(int docId, Scope scope, IndexReader reader, BinaryDocValues values, out Guid result)
        {
            result = Guid.Empty;

            if (!TryGet(docId, values, out var @for))
            {
                return false;
            }

            if (scope == Scope.Draft && @for[ForDraftIndex] != 1)
            {
                return false;
            }

            if (scope == Scope.Published && @for[ForPublishedIndex] != 1)
            {
                return false;
            }

            var document = reader.Document(docId);

            var idString = document.Get(MetaId);

            if (!Guid.TryParse(idString, out result))
            {
                return false;
            }

            return true;
        }

        public void Index(NamedContentData dataDraft, NamedContentData data, BinaryDocValues values, bool onlyDraft)
        {
            var converted = CreateDocument(dataDraft);

            Upsert(converted, 1, 1, 0);

            var docId = GetPublishedDocument();

            if (data != null)
            {
                Upsert(CreateDocument(data), 0, 0, 1);
            }
            else
            {
                var isPublished = IsForPublished(docId, values);

                if (!onlyDraft && docId > 0 && isPublished)
                {
                    Upsert(converted, 0, 0, 1);
                }
                else if (!onlyDraft)
                {
                    Upsert(converted, 0, 0, 0);
                }
                else
                {
                    Update(0, 0, isPublished ? (byte)1 : (byte)0);
                }
            }
        }

        public void Copy(bool fromDraft)
        {
            if (fromDraft)
            {
                Update(1, 1, 0);
                Update(0, 0, 1);
            }
            else
            {
                Update(1, 0, 0);
                Update(0, 1, 1);
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

        private int GetPublishedDocument()
        {
            var docs = indexSearcher?.Search(new TermQuery(new Term(MetaKey, BuildKey(0))), 1);

            return docs?.ScoreDocs.FirstOrDefault()?.Doc ?? 0;
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

                // indexWriter.DeleteDocuments(new Term(MetaKey, docKey));
                indexWriter.UpdateDocument(new Term(MetaKey, docKey), document);
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

        private static bool IsForPublished(int docId, BinaryDocValues values)
        {
            return TryGet(docId, values, out var @for) && @for[1] == 1;
        }

        private static bool TryGet(int docId, BinaryDocValues values, out byte[] result)
        {
            var forValue = new BytesRef();

            values?.Get(docId, forValue);

            if (forValue.Bytes.Length == 2)
            {
                result = forValue.Bytes;
                return true;
            }
            else
            {
                result = null;
                return false;
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
