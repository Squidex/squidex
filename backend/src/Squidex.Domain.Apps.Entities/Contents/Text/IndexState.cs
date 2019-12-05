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
        private const string MetaFor = "_fd";
        private readonly Dictionary<(Guid, Scope), (bool, bool)> lastChanges = new Dictionary<(Guid, Scope), (bool, bool)>();
        private readonly BytesRef bytesRef = new BytesRef(2);
        private readonly IIndex index;

        public IndexState(IIndex index)
        {
            this.index = index;
        }

        public void Index(Guid id, Scope scope, Document document, bool forDraft, bool forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            document.SetBinaryDocValue(MetaFor, value);

            lastChanges[(id, scope)] = (forDraft, forPublished);
        }

        public void Index(Guid id, Scope scope, Term term, bool forDraft, bool forPublished)
        {
            var value = GetValue(forDraft, forPublished);

            index.Writer.UpdateBinaryDocValue(term, MetaFor, value);

            lastChanges[(id, scope)] = (forDraft, forPublished);
        }

        public bool HasBeenAdded(Guid id, Scope scope, Term term, out int docId)
        {
            docId = -1;

            if (lastChanges.ContainsKey((id, scope)))
            {
                return true;
            }

            if (index.Searcher == null)
            {
                return false;
            }

            var docs = index.Searcher.Search(new TermQuery(term), 1);

            var found = docs.ScoreDocs.FirstOrDefault();

            if (found != null)
            {
                docId = found.Doc;

                return true;
            }

            return false;
        }

        public void Get(Guid id, Scope scope, int docId, out bool forDraft, out bool forPublished)
        {
            if (lastChanges.TryGetValue((id, scope), out var forValue))
            {
                (forDraft, forPublished) = forValue;
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
            return index.Reader.GetBinaryValue(MetaFor, docId, bytesRef);
        }

        private static BytesRef GetValue(bool forDraft, bool forPublished)
        {
            return new BytesRef(new[]
            {
                (byte)(forDraft ? 1 : 0),
                (byte)(forPublished ? 1 : 0)
            });
        }

        private static (bool, bool) ToFlags(BytesRef bytes)
        {
            return (bytes.Bytes[0] == 1, bytes.Bytes[1] == 1);
        }
    }
}
