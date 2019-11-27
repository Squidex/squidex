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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal sealed class TextIndexContent
    {
        private const string MetaId = "_id";
        private const string MetaKey = "_key";
        private readonly IndexHolder index;
        private readonly IndexState indexState;
        private readonly Guid id;

        public TextIndexContent(IndexHolder index, IndexState indexState, Guid id)
        {
            this.index = index;
            this.indexState = indexState;

            this.id = id;
        }

        public void Delete()
        {
            index.Writer.DeleteDocuments(new Term(MetaId, id.ToString()));
        }

        public static bool TryGetId(int docId, Scope scope, IndexHolder index, IndexState indexState, out Guid result)
        {
            result = Guid.Empty;

            indexState.Get(docId, out var draft, out var published);

            if (scope == Scope.Draft && draft != 1)
            {
                return false;
            }

            if (scope == Scope.Published && published != 1)
            {
                return false;
            }

            var document = index.GetSearcher(true)!.Doc(docId);

            if (document != null)
            {
                var idString = document.Get(MetaId);

                if (!Guid.TryParse(idString, out result))
                {
                    return false;
                }
            }

            return true;
        }

        public void Index(NamedContentData data, bool onlyDraft)
        {
            var converted = CreateDocument(data);

            Upsert(converted, 1, 1, 0);

            var isPublishDocumentAdded = IsAdded(0, out var docId);
            var isPublishForPublished = IsForPublished(0, docId);

            if (!onlyDraft && isPublishDocumentAdded && isPublishForPublished)
            {
                Upsert(converted, 0, 0, 1);
            }
            else if (!onlyDraft || !isPublishDocumentAdded)
            {
                Upsert(converted, 0, 0, 0);
            }
            else
            {
                UpdateFor(0, 0, isPublishForPublished ? (byte)1 : (byte)0);
            }
        }

        public void Copy(bool fromDraft)
        {
            if (fromDraft)
            {
                UpdateFor(1, 1, 0);
                UpdateFor(0, 0, 1);
            }
            else
            {
                UpdateFor(1, 0, 0);
                UpdateFor(0, 1, 1);
            }
        }

        private static Document CreateDocument(NamedContentData data)
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
                if (field.Value != null)
                {
                    foreach (var fieldValue in field.Value)
                    {
                        var appendText = new Action<string>(text => AppendText(fieldValue.Key, text));

                        AppendJsonText(fieldValue.Value, appendText);
                    }
                }
            }

            var document = new Document();

            foreach (var field in languages)
            {
                document.AddTextField(field.Key, field.Value.ToString(), Field.Store.NO);
            }

            return document;
        }

        private void UpdateFor(byte draft, byte forDraft, byte forPublished)
        {
            var term = new Term(MetaKey, BuildKey(draft));

            indexState.Index(id, draft, term, forDraft, forPublished);
        }

        private void Upsert(Document document, byte draft, byte forDraft, byte forPublished)
        {
            if (document != null)
            {
                document.RemoveField(MetaId);
                document.RemoveField(MetaKey);

                var contentId = id.ToString();
                var contentKey = BuildKey(draft);

                document.AddStringField(MetaId, contentId, Field.Store.YES);
                document.AddStringField(MetaKey, contentKey, Field.Store.YES);

                indexState.Index(id, draft, document, forDraft, forPublished);

                index.Writer.UpdateDocument(new Term(MetaKey, contentKey), document);
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

        private bool IsAdded(byte draft, out int docId)
        {
            var term = new Term(MetaKey, BuildKey(draft));

            return indexState.HasBeenAdded(id, draft, term, out docId);
        }

        private bool IsForPublished(byte draft, int docId)
        {
            indexState.Get(id, draft, docId, out _, out var forPublished);

            return forPublished == 1;
        }

        private string BuildKey(byte draft)
        {
            return $"{id}_{draft}";
        }
    }
}
