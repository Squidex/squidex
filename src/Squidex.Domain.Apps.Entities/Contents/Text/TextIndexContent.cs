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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal sealed class TextIndexContent
    {
        public const string MetaId = "_id";
        public const string MetaKey = "_key";
        public const string MetaDraft = "_dd";

        private readonly IndexWriter indexWriter;
        private readonly IndexSearcher indexSearcher;
        private readonly Guid id;

        public TextIndexContent(IndexWriter indexWriter, IndexSearcher indexSearcher, Guid id)
        {
            this.indexWriter = indexWriter;
            this.indexSearcher = indexSearcher;

            this.id = id;
        }

        public void Index(NamedContentData data, bool isDraft)
        {
            var converted = CreateDocument(data);

            Upsert(converted, isDraft);
        }

        public void Delete()
        {
            indexWriter.DeleteDocuments(new Term(MetaId, id.ToString()));
        }

        public void Copy(bool fromDraft)
        {
            var published = GetDocument(fromDraft);

            Upsert(published, !fromDraft);
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

        private Document GetDocument(bool draft)
        {
            if (indexSearcher == null)
            {
                return null;
            }

            var docs = indexSearcher.Search(new TermQuery(new Term(MetaKey, BuildKey(BuildValue(draft)))), 1);

            if (docs.ScoreDocs.Length > 0)
            {
                return indexSearcher.Doc(docs.ScoreDocs[0].Doc);
            }

            return null;
        }

        private void Upsert(Document document, bool draft)
        {
            if (document != null)
            {
                document.RemoveField(MetaId);
                document.RemoveField(MetaKey);
                document.RemoveField(MetaDraft);

                var docDraft = BuildValue(draft);

                var docId = id.ToString();
                var docKey = BuildKey(docDraft);

                document.AddStringField(MetaId, docId, Field.Store.YES);
                document.AddStringField(MetaKey, docKey, Field.Store.YES);
                document.AddStringField(MetaDraft, docDraft, Field.Store.YES);

                indexWriter.DeleteDocuments(new Term(MetaKey, docKey));
                indexWriter.AddDocument(document);
            }
        }

        private static string BuildValue(bool draft)
        {
            return draft ? "1" : "0";
        }

        private string BuildKey(string draft)
        {
            return $"{id}_{draft}";
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
    }
}
