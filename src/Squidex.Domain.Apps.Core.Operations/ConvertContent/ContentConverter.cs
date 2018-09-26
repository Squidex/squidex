// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverter
    {
        private static readonly Func<IRootField, string> KeyNameResolver = f => f.Name;
        private static readonly Func<IRootField, long> KeyIdResolver = f => f.Id;

        public static NamedContentData ConvertId2Name(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsById, KeyNameResolver, converters);
        }

        public static IdContentData ConvertId2Id(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsById, KeyIdResolver, converters);
        }

        public static NamedContentData ConvertName2Name(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsByName, KeyNameResolver, converters);
        }

        public static IdContentData ConvertName2Id(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsByName, KeyIdResolver, converters);
        }

        private static TDict2 ConvertInternal<TKey1, TKey2, TDict1, TDict2>(
            TDict1 source,
            TDict2 target,
            IReadOnlyDictionary<TKey1, RootField> fields,
            Func<IRootField, TKey2> targetKey, params FieldConverter[] converters)
            where TDict1 : IDictionary<TKey1, ContentFieldData>
            where TDict2 : IDictionary<TKey2, ContentFieldData>
        {
            foreach (var fieldKvp in source)
            {
                if (!fields.TryGetValue(fieldKvp.Key, out var field))
                {
                    continue;
                }

                var newvalue = fieldKvp.Value;

                if (converters != null)
                {
                    foreach (var converter in converters)
                    {
                        newvalue = converter(newvalue, field);

                        if (newvalue == null)
                        {
                            break;
                        }
                    }
                }

                if (newvalue != null)
                {
                    target.Add(targetKey(field), newvalue);
                }
            }

            return target;
        }
    }
}
