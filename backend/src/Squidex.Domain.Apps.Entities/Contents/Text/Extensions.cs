// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Lucene.Net.Documents;
using Lucene.Net.Util;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
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
    }
}
