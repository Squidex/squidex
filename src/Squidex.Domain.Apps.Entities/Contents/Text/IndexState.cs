// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Util;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    internal sealed class IndexState
    {
        private const string MetaFor = "_fd";
        private readonly IndexReader indexReader;
        private readonly IndexWriter indexWriter;
        private readonly BinaryDocValues binaryValues;

        public IndexState(IndexReader indexReader, IndexWriter indexWriter)
        {
            this.indexReader = indexReader;
            this.indexWriter = indexWriter;

            if (indexReader != null)
            {
                binaryValues = MultiDocValues.GetBinaryValues(indexReader, MetaFor);
            }
        }

        public void Index(Document document, byte forDraft, byte forPublished)
        {
            document.RemoveField(MetaFor);
            document.AddBinaryDocValuesField(MetaFor, GetValue(forDraft, forPublished));
        }

        public void Index(Term term, Guid id, byte draft, byte forDraft, byte forPublished)
        {
            indexWriter.UpdateBinaryDocValue(term, MetaFor, GetValue(forDraft, forPublished));
        }

        public bool TryGet(int docId, out byte forDraft, out byte forPublished)
        {
            var forValue = new BytesRef();

            forDraft = 0;
            forPublished = 0;

            binaryValues?.Get(docId, forValue);

            if (forValue.Bytes.Length == 2)
            {
                forDraft = forValue.Bytes[0];
                forPublished = forValue.Bytes[1];

                return true;
            }

            return false;
        }

        private static BytesRef GetValue(byte forDraft, byte forPublished)
        {
            return new BytesRef(new byte[] { forDraft, forPublished });
        }
    }
}
