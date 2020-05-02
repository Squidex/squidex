// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverter
    {
        private static readonly Func<IRootField, string> KeyNameResolver = f => f.Name;
        private static readonly Func<IRootField, long> KeyIdResolver = f => f.Id;

        public static void Convert(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema);

            if (converters?.Any() != true)
            {
                return;
            }

            foreach (var (fieldName, data) in content.ToList())
            {
                ContentFieldData? newData = data;

                if (schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    for (var i = 0; i < converters.Length; i++)
                    {
                        newData = converters[i](newData!, field);

                        if (newData == null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    newData = null;
                }

                if (newData == null)
                {
                    content.Remove(fieldName);
                }
                else if (!ReferenceEquals(data, newData))
                {
                    content[fieldName] = newData;
                }
            }
        }

        public static NamedContentData ConvertId2Name(this IdContentData content, Schema schema)
        {
            Guard.NotNull(schema);

            var result = new NamedContentData(content.Count);

            foreach (var (fieldName, value) in content)
            {
                if (schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    if (newValue != null)
                    {
                        target.Add(targetKey(field), newValue);
                    }
                }
            }

            return result;
        }

        public static IdContentData ConvertName2Id(this NamedContentData content, Schema schema)
        {
            Guard.NotNull(schema);

            var result = new IdContentData(content.Count);

            foreach (var (fieldName, value) in content)
            {
                if (schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    var clone = value.Clone();

                    if (field is IArrayField arrayField && field.)
                }
            }

            return result;
        }

        private static TDict2 ConvertInternal<TKey1, TKey2, TDict1, TDict2>(
            TDict1 source,
            TDict2 target,
            IReadOnlyDictionary<TKey1, RootField> fields,
            Func<IRootField, TKey2> targetKey)
            where TDict1 : IDictionary<TKey1, ContentFieldData?>
            where TDict2 : IDictionary<TKey2, ContentFieldData?>
            where TKey1 : notnull
            where TKey2 : notnull
        {
            foreach (var (fieldName, value) in source)
            {
                if (fields.TryGetValue(fieldName, out var field))
                {
                    if (field is IArrayField arra)
                    var newValue = value;

                    if (newValue != null)
                    {
                        target.Add(targetKey(field), newValue);
                    }
                }
            }

            return target;
        }
    }
}