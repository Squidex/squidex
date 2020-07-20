// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverter
    {
        public static NamedContentData ConvertId2Name(this IdContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

            foreach (var (fieldId, data) in content)
            {
                if (data == null || !schema.FieldsById.TryGetValue(fieldId, out var field))
                {
                    continue;
                }

                ContentFieldData? newData = data;

                ConvertArray(newData, field, FieldIdentifier.ById, FieldIdentifier.ByName);

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

        public static NamedContentData ConvertName2Name(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData(content.Count);

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

        public static IdContentData ConvertName2IdCloned(this NamedContentData content, Schema schema, params FieldConverter[] converters)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData(content.Count);

            foreach (var (fieldName, data) in content)
            {
                if (data == null || !schema.FieldsByName.TryGetValue(fieldName, out var field))
                {
                    continue;
                }

                ContentFieldData? newData = data.Clone();

                if (newData != null)
                {
                    newData = ConvertData(converters, field, newData);
                }

                if (newData != null)
                {
                    ConvertArray(newData, field, FieldIdentifier.ByName, FieldIdentifier.ById);
                }

                if (newData != null)
                {
                    result.Add(field.Id, newData);
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

        private static void ConvertArray(ContentFieldData data, IRootField? field, FieldIdentifier sourceIdentifier, FieldIdentifier targetIdentifier)
        {
            if (field is IArrayField arrayField)
            {
                foreach (var (_, value) in data)
                {
                    if (value is JsonArray array)
                    {
                        foreach (var nested in array.OfType<JsonObject>())
                        {
                            var properties = nested.ToList();

                            nested.Clear();

                            foreach (var (nestedKey, nestedValue) in properties)
                            {
                                if (nestedValue == null)
                                {
                                    continue;
                                }

                                var nestedField = sourceIdentifier.GetField(arrayField, nestedKey);

                                if (nestedField == null)
                                {
                                    continue;
                                }

                                var targetKey = targetIdentifier.GetStringKey(nestedField);

                                nested[targetKey] = nestedValue;
                            }
                        }
                    }
                }
            }
        }
    }
}