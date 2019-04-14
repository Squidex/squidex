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
        private readonly IndexSearcher indexSearcher;
        private readonly IndexWriter indexWriter;
        private readonly BinaryDocValues binaryValues;
        private readonly Dictionary<(Guid, byte), BytesRef> changes = new Dictionary<(Guid, byte), BytesRef>();
        private bool isClosed;

        public int Changes
        {
            get { return changes.Count; }
        }

        public IndexState(IndexWriter indexWriter, IndexReader indexReader = null, IndexSearcher indexSearcher = null)
        {
            this.indexSearcher = indexSearcher;
            this.indexWriter = indexWriter;

            if (indexReader != null)
            {
                binaryValues = MultiDocValues.GetBinaryValues(indexReader, MetaFor);
            }
        }

        public void Index(Guid id, byte draft, Document document, byte forDraft, byte forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            document.RemoveField(MetaFor);
            document.AddBinaryDocValuesField(MetaFor, value);

            changes[(id, draft)] = value;
        }

        public void Index(Guid id, byte draft, Term term, byte forDraft, byte forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            indexWriter.UpdateBinaryDocValue(term, MetaFor, value);

            changes[(id, draft)] = value;
        }

        public bool HasBeenAdded(Guid id, byte draft, Term term, out int docId)
        {
            docId = 0;

            if (changes.ContainsKey((id, draft)))
            {
                return true;
            }

            if (indexSearcher != null && !isClosed)
            {
                var docs = indexSearcher.Search(new TermQuery(term), 1);

                docId = docs?.ScoreDocs.FirstOrDefault()?.Doc ?? NotFound;

                return docId > NotFound;
            }

            return false;
        }

        public bool TryGet(Guid id, byte draft, int docId, out byte forDraft, out byte forPublished)
        {
            forDraft = 0;
            forPublished = 0;

            if (changes.TryGetValue((id, draft), out var forValue))
            {
                forDraft = forValue.Bytes[0];
                forPublished = forValue.Bytes[1];

                return true;
            }

            if (!isClosed && docId != NotFound)
            {
                forValue = new BytesRef();

                binaryValues?.Get(docId, forValue);

                if (forValue.Bytes.Length == 2)
                {
                    forDraft = forValue.Bytes[0];
                    forPublished = forValue.Bytes[1];

                    changes[(id, draft)] = forValue;

                    return true;
                }
            }

            return false;
        }

        public bool TryGet(int docId, out byte forDraft, out byte forPublished)
        {
            forDraft = 0;
            forPublished = 0;

            if (!isClosed && docId != NotFound)
            {
                var forValue = new BytesRef();

                binaryValues?.Get(docId, forValue);

                if (forValue.Bytes.Length == 2)
                {
                    forDraft = forValue.Bytes[0];
                    forPublished = forValue.Bytes[1];

                    return true;
                }
            }

            return false;
        }

        private static BytesRef GetValue(byte forDraft, byte forPublished)
        {
            return new BytesRef(new[] { forDraft, forPublished });
        }

        public void CloseReader()
        {
            isClosed = true;
        }
    }
}
