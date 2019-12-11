﻿// ==========================================================================
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
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

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

        public static Dictionary<string, string> ToTexts(this NamedContentData data)
        {
            var result = new Dictionary<string, string>();

            if (data != null)
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

                foreach (var value in data.Values)
                {
                    if (value != null)
                    {
                        foreach (var (key, jsonValue) in value)
                        {
                            var appendText = new Action<string>(text => AppendText(key, text));

                            AppendJsonText(jsonValue, appendText);
                        }
                    }
                }

                foreach (var (key, value) in languages)
                {
                    result[key] = value.ToString();
                }
            }

            return result;
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
