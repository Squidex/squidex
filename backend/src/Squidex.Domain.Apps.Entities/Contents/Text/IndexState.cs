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
        private readonly Dictionary<(Guid, Scope), BytesRef> lastChanges = new Dictionary<(Guid, Scope), BytesRef>();
        private readonly IndexHolder index;
        private IndexReader? lastReader;
        private BinaryDocValues binaryValues;

        public IndexState(IndexHolder index)
        {
            this.index = index;
        }

        public void Index(Guid id, Scope scope, Document document, bool forDraft, bool forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            document.SetBinaryDocValue(MetaFor, value);

            lastChanges[(id, scope)] = value;
        }

        public void Index(Guid id, Scope scope, Term term, bool forDraft, bool forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            index.Writer.UpdateBinaryDocValue(term, MetaFor, value);

            lastChanges[(id, scope)] = value;
        }

        public bool HasBeenAdded(Guid id, Scope scope, Term term, out int docId)
        {
            docId = 0;

            if (lastChanges.ContainsKey((id, scope)))
            {
                return true;
            }

            var docs = index.Searcher?.Search(new TermQuery(term), 1);

            docId = docs?.ScoreDocs.FirstOrDefault()?.Doc ?? NotFound;

            return docId > NotFound;
        }

        public void Get(Guid id, Scope scope, int docId, out bool forDraft, out bool forPublished)
        {
            if (lastChanges.TryGetValue((id, scope), out var forValue))
            {
                (forDraft, forPublished) = ToFlags(forValue);
            }
            else
            {
                Get(docId, out forDraft, out forPublished);
            }
        }

        public void Get(int docId, out bool forDraft, out bool forPublished)
        {
            var forValue = GetForValues(docId);

            (forDraft, forPublished) = ToFlags(forValue);
        }

        private BytesRef GetForValues(int docId)
        {
            var reader = index.Reader;

            if (lastReader != reader)
            {
                lastChanges.Clear();
                lastReader = reader;

                if (reader != null)
                {
                    binaryValues = MultiDocValues.GetBinaryValues(reader, MetaFor);
                }
            }

            var result = new BytesRef(2);

            if (docId != NotFound)
            {
                binaryValues?.Get(docId, result);
            }

            return result;
        }

        private static BytesRef GetValue(bool forDraft, bool forPublished)
        {
            return GetValue((byte)(forDraft ? 1 : 0), (byte)(forPublished ? 1 : 0));
        }

        private static BytesRef GetValue(byte forDraft, byte forPublished)
        {
            return new BytesRef(new[] { forDraft, forPublished });
        }

        private static (bool, bool) ToFlags(BytesRef bytes)
        {
            return (bytes.Bytes[0] == 1, bytes.Bytes[1] == 1);
        }
    }
}
