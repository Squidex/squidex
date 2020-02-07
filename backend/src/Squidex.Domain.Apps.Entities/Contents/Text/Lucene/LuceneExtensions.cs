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

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public static class LuceneExtensions
    {
        public static void SetBinaryDocValue(this Document document, string name, BytesRef value)
        {
            document.RemoveField(name);

            document.AddBinaryDocValuesField(name, value);
        }

        public static void SetField(this Document document, string name, string value)
        {
            document.RemoveField(name);

            document.AddStringField(name, value, Field.Store.YES);
        }

        public static BytesRef GetBinaryValue(this IndexReader? reader, string field, int docId, BytesRef? result = null)
        {
            if (result != null)
            {
                Array.Clear(result.Bytes, 0, result.Bytes.Length);
            }
            else
            {
                result = new BytesRef();
            }

            if (reader == null || docId < 0)
            {
                return result;
            }

            var leaves = reader.Leaves;

            if (leaves.Count == 1)
            {
                var docValues = leaves[0].AtomicReader.GetBinaryDocValues(field);

                docValues.Get(docId, result);
            }
            else if (leaves.Count > 1)
            {
                var subIndex = ReaderUtil.SubIndex(docId, leaves);

                var subLeave = leaves[subIndex];
                var subValues = subLeave.AtomicReader.GetBinaryDocValues(field);

                subValues.Get(docId - subLeave.DocBase, result);
            }

            return result;
        }
    }
}
