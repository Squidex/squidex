// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Lucene.Net.Documents;
using Lucene.Net.Index;

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

            if (scope == Scope.Draft && !draft)
            {
                return false;
            }

            if (scope == Scope.Published && !published)
            {
                return false;
            }

            var document = index.Searcher?.Doc(docId);

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

        public void Index(TextContent text, bool onlyDraft)
        {
            var converted = CreateDocument(text);

            Upsert(converted, Scope.Draft,
                forDraft: true,
                forPublished: false);

            var isPublishDocumentAdded = IsAdded(Scope.Published, out var docId);
            var isPublishForPublished = IsForPublished(Scope.Published, docId);

            if (!onlyDraft && isPublishDocumentAdded && isPublishForPublished)
            {
                Upsert(converted, Scope.Published,
                    forDraft: false,
                    forPublished: true);
            }
            else if (!onlyDraft || !isPublishDocumentAdded)
            {
                Upsert(converted, Scope.Published,
                    forDraft: false,
                    forPublished: false);
            }
            else
            {
                UpdateFor(Scope.Published,
                    forDraft: false,
                    forPublished: isPublishForPublished);
            }
        }

        public void Copy(bool fromDraft)
        {
            if (fromDraft)
            {
                UpdateFor(Scope.Draft,
                    forDraft: true,
                    forPublished: false);

                UpdateFor(Scope.Published,
                    forDraft: false,
                    forPublished: true);
            }
            else
            {
                UpdateFor(Scope.Draft,
                    forDraft: false,
                    forPublished: false);

                UpdateFor(Scope.Published,
                    forDraft: true,
                    forPublished: true);
            }
        }

        private static Document CreateDocument(TextContent text)
        {
            var document = new Document();

            foreach (var field in text)
            {
                document.AddTextField(field.Key, field.Value, Field.Store.NO);
            }

            return document;
        }

        private void UpdateFor(Scope scope, bool forDraft, bool forPublished)
        {
            var term = new Term(MetaKey, BuildKey(scope));

            indexState.Index(id, scope, term, forDraft, forPublished);
        }

        private void Upsert(Document document, Scope draft, bool forDraft, bool forPublished)
        {
            var contentKey = BuildKey(draft);

            document.SetField(MetaId, id.ToString());
            document.SetField(MetaKey, contentKey);

            indexState.Index(id, draft, document, forDraft, forPublished);

            index.Writer.UpdateDocument(new Term(MetaKey, contentKey), document);
        }

        private bool IsAdded(Scope scope, out int docId)
        {
            var term = new Term(MetaKey, BuildKey(scope));

            return indexState.HasBeenAdded(id, scope, term, out docId);
        }

        private bool IsForPublished(Scope scope, int docId)
        {
            indexState.Get(id, scope, docId, out _, out var forPublished);

            return forPublished;
        }

        private string BuildKey(Scope scope)
        {
            return $"{id}_{(scope == Scope.Draft ? 1 : 0)}";
        }
    }
}
