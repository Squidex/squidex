// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal sealed class IndexState
    {
        private const int NotFound = -1;
        private const string MetaFor = "_fd";
        private readonly Dictionary<(Guid, byte), BytesRef> lastChanges = new Dictionary<(Guid, byte), BytesRef>();
        private readonly IndexHolder index;
        private IndexReader? lastReader;
        private BinaryDocValues binaryValues;

        public IndexState(IndexHolder index)
        {
            this.index = index;
        }

        public void Commit()
        {
            lastChanges.Clear();
        }

        public void Index(Guid id, byte draft, Document document, byte forDraft, byte forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            document.SetBinaryDocValue(MetaFor, value);

            lastChanges[(id, draft)] = value;
        }

        public void Index(Guid id, byte draft, Term term, byte forDraft, byte forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            index.Writer.UpdateBinaryDocValue(term, MetaFor, value);

            lastChanges[(id, draft)] = value;
        }

        public bool HasBeenAdded(Guid id, byte draft, Term term, out int docId)
        {
            docId = 0;

            if (lastChanges.ContainsKey((id, draft)))
            {
                return true;
            }

            var docs = index.Searcher.Search(new TermQuery(term), 1);

            docId = docs?.ScoreDocs.FirstOrDefault()?.Doc ?? NotFound;

            return docId > NotFound;
        }

        public void Get(Guid id, byte draft, int docId, out byte forDraft, out byte forPublished)
        {
            if (lastChanges.TryGetValue((id, draft), out var forValue))
            {
                forDraft = forValue.Bytes[0];
                forPublished = forValue.Bytes[1];
                return;
            }

            forValue = GetForValues(docId);

            forDraft = forValue.Bytes[0];
            forPublished = forValue.Bytes[1];

            lastChanges[(id, draft)] = forValue;
        }

        public void Get(int docId, out byte forDraft, out byte forPublished)
        {
            var forValue = GetForValues(docId);

            forDraft = forValue.Bytes[0];
            forPublished = forValue.Bytes[1];
        }

        private BytesRef GetForValues(int docId)
        {
            if (lastReader != index.Reader)
            {
                lastChanges.Clear();
                lastReader = index.Reader;

                binaryValues = MultiDocValues.GetBinaryValues(index.Reader, MetaFor);
            }

            var result = new BytesRef(2);

            if (docId != NotFound)
            {
                binaryValues?.Get(docId, result);
            }

            return result;
        }

        private static BytesRef GetValue(byte forDraft, byte forPublished)
        {
            return new BytesRef(new[] { forDraft, forPublished });
        }
    }
}
