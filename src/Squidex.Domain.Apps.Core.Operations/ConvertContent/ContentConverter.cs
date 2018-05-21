// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData FieldConverter(ContentFieldData data, Field field);

    public static class ContentConverter
    {
        public static NamedContentData ToNameModel(this IdContentData source, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData();

            foreach (var fieldValue in source)
            {
                if (!schema.FieldsById.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                var fieldData = Convert(fieldValue.Value, field, converters);

                if (fieldData != null)
                {
                    result[field.Name] = fieldData;
                }
            }

            return result;
        }

        public static IdContentData ToIdModel(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData();

            foreach (var fieldValue in content)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                var fieldData = Convert(fieldValue.Value, field, converters);

                if (fieldData != null)
                {
                    result[field.Id] = fieldData;
                }
            }

            return result;
        }

        public static IdContentData Convert(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData();

            foreach (var fieldValue in content)
            {
                if (!schema.FieldsById.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                var fieldData = Convert(fieldValue.Value, field, converters);

                if (fieldData != null)
                {
                    result[field.Id] = fieldData;
                }
            }

            return result;
        }

        public static NamedContentData Convert(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData();

            foreach (var fieldValue in content)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                var fieldData = Convert(fieldValue.Value, field, converters);

                if (fieldData != null)
                {
                    result[field.Name] = fieldData;
                }
            }

            return result;
        }

        private static ContentFieldData Convert(ContentFieldData fieldData, Field field, FieldConverter[] converters)
        {
            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    fieldData = converter(fieldData, field);

                    if (fieldData == null)
                    {
                        break;
                    }
                }
            }

            return fieldData;
        }
    }
}
