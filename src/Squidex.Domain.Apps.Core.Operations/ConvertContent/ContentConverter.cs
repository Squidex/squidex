// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData FieldConverter(ContentFieldData data, IRootField field);

    public delegate JToken ValueConverter(JToken value, IRootField field);

    public static class ContentConverter
    {
        public static NamedContentData ToNameModel(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsById, x => x.Name, converters);
        }

        public static IdContentData ToIdModel(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsByName, x => x.Id, converters);
        }

        public static IdContentData Convert(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsById, x => x.Id, converters);
        }

        public static NamedContentData Convert(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

            return ConvertInternal(content, result, schema.FieldsByName, x => x.Name, converters);
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

                var fieldValue = fieldKvp.Value;

                if (converters != null)
                {
                    foreach (var converter in converters)
                    {
                        fieldValue = converter(fieldValue, field);

                        if (fieldValue == null)
                        {
                            break;
                        }
                    }
                }

                if (fieldValue != null)
                {
                    target.Add(targetKey(field), fieldValue);
                }
            }

            return target;
        }
    }
}
