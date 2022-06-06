// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverter
    {
        public static ContentData Convert(this ContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema);

            var result = new ContentData(content.Count);

            if (converters == null || converters.Length == 0)
            {
                return result;
            }

            foreach (var (fieldName, fieldData) in content)
            {
                if (fieldData == null || !schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    continue;
                }

                ContentFieldData? newData = fieldData;

                if (newData != null)
                {
                    newData = ConvertData(field, newData, converters);
                }

                if (newData != null)
                {
                    result.Add(field.Name, newData);
                }
            }

            return result;
        }

        private static ContentFieldData? ConvertData(IRootField field, ContentFieldData data, FieldConverter[] converters)
        {
            if (converters == null || converters.Length == 0)
            {
                return data;
            }

            foreach (var converter in converters)
            {
                data = converter(data!, field)!;

                if (data == null)
                {
                    break;
                }
            }

            return data;
        }
    }
}
