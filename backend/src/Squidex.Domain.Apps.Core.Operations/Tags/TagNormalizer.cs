// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Tags
{
    public static class TagNormalizer
    {
        public static async Task NormalizeAsync(this ITagService tagService, DomainId appId, DomainId schemaId, Schema schema, ContentData newData, ContentData? oldData)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(newData, nameof(newData));

            var newValues = new HashSet<string>();
            var newArrays = new List<JsonArray>();

            var oldValues = new HashSet<string>();
            var oldArrays = new List<JsonArray>();

            GetValues(schema, newValues, newArrays, newData);

            if (oldData != null)
            {
                GetValues(schema, oldValues, oldArrays, oldData);
            }

            if (newValues.Count > 0)
            {
                var normalized = await tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId), newValues, oldValues);

                foreach (var array in newArrays)
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        if (normalized.TryGetValue(array[i].ToString(), out var result))
                        {
                            array[i] = JsonValue.Create(result);
                        }
                    }
                }
            }
        }

        public static async Task DenormalizeAsync(this ITagService tagService, DomainId appId, DomainId schemaId, Schema schema, params ContentData[] datas)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(schema, nameof(schema));

            var tagsValues = new HashSet<string>();
            var tagsArrays = new List<JsonArray>();

            GetValues(schema, tagsValues, tagsArrays, datas);

            if (tagsValues.Count > 0)
            {
                var denormalized = await tagService.DenormalizeTagsAsync(appId, TagGroups.Schemas(schemaId), tagsValues);

                foreach (var array in tagsArrays)
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        if (denormalized.TryGetValue(array[i].ToString(), out var result))
                        {
                            array[i] = JsonValue.Create(result);
                        }
                    }
                }
            }
        }

        private static void GetValues(Schema schema, HashSet<string> values, List<JsonArray> arrays, params ContentData[] datas)
        {
            foreach (var field in schema.Fields)
            {
                if (field is IField<TagsFieldProperties> tags && tags.Properties.Normalization == TagsFieldNormalization.Schema)
                {
                    foreach (var data in datas)
                    {
                        if (data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                        {
                            foreach (var partition in fieldData)
                            {
                                ExtractTags(partition.Value, values, arrays);
                            }
                        }
                    }
                }
                else if (field is IArrayField arrayField)
                {
                    foreach (var nestedField in arrayField.Fields)
                    {
                        if (nestedField is IField<TagsFieldProperties> nestedTags && nestedTags.Properties.Normalization == TagsFieldNormalization.Schema)
                        {
                            foreach (var data in datas)
                            {
                                if (data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                                {
                                    foreach (var partition in fieldData)
                                    {
                                        if (partition.Value is JsonArray array)
                                        {
                                            foreach (var value in array)
                                            {
                                                if (value is JsonObject nestedObject)
                                                {
                                                    if (nestedObject.TryGetValue(nestedField.Name, out var nestedValue))
                                                    {
                                                        ExtractTags(nestedValue, values, arrays);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ExtractTags(IJsonValue value, ISet<string> values, ICollection<JsonArray> arrays)
        {
            if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item.Type == JsonValueType.String)
                    {
                        values.Add(item.ToString());
                    }
                }

                arrays.Add(array);
            }
        }
    }
}
