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
    public static class ContentConverter
    {
        public static ContentData Convert(this ContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData(content.Count);

            foreach (var (fieldName, data) in content)
            {
                if (data == null || !schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    continue;
                }

                ContentFieldData? newData = data;

                if (newData != null)
                {
                    newData = ConvertData(converters, field, newData);
                }

                if (newData != null)
                {
                    result.Add(field.Name, newData);
                }
            }

            return result;
        }

        private static ContentFieldData? ConvertData(FieldConverter[] converters, IRootField field, ContentFieldData data)
        {
            if (converters != null)
            {
                for (var i = 0; i < converters.Length; i++)
                {
                    data = converters[i](data!, field)!;

                    if (data == null)
                    {
                        break;
                    }
                }
            }

            return data;
        }
    }
}