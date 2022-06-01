// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            Guard.NotNull(tagService);
            Guard.NotNull(schema);
            Guard.NotNull(newData);

            var newValues = new HashSet<string>();
            var newArrays = new List<JsonValue2>();

            var oldValues = new HashSet<string>();
            var oldArrays = new List<JsonValue2>();

            GetValues(schema, newValues, newArrays, newData);

            if (oldData != null)
            {
                GetValues(schema, oldValues, oldArrays, oldData);
            }

            if (newValues.Count > 0)
            {
                var normalized = await tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId), newValues, oldValues);

                foreach (var source in newArrays)
                {
                    var array = source.AsArray;

                    for (var i = 0; i < array.Count; i++)
                    {
                        if (normalized.TryGetValue(array[i].ToString(), out var result))
                        {
                            array[i] = result;
                        }
                    }
                }
            }
        }

        public static async Task DenormalizeAsync(this ITagService tagService, DomainId appId, DomainId schemaId, Schema schema, params ContentData[] datas)
        {
            Guard.NotNull(tagService);
            Guard.NotNull(schema);

            var tagsValues = new HashSet<string>();
            var tagsArrays = new List<JsonValue2>();

            GetValues(schema, tagsValues, tagsArrays, datas);

            if (tagsValues.Count > 0)
            {
                var denormalized = await tagService.DenormalizeTagsAsync(appId, TagGroups.Schemas(schemaId), tagsValues);

                foreach (var source in tagsArrays)
                {
                    var array = source.AsArray;

                    for (var i = 0; i < array.Count; i++)
                    {
                        if (denormalized.TryGetValue(array[i].ToString(), out var result))
                        {
                            array[i] = result;
                        }
                    }
                }
            }
        }

        private static void GetValues(Schema schema, HashSet<string> values, List<JsonValue2> arrays, params ContentData[] datas)
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
                                        if (partition.Value.Type == JsonValueType.Array)
                                        {
                                            foreach (var value in partition.Value.AsArray)
                                            {
                                                if (value.Type == JsonValueType.Object)
                                                {
                                                    if (value.AsObject.TryGetValue(nestedField.Name, out var nestedValue))
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

        private static void ExtractTags(JsonValue2 value, ISet<string> values, ICollection<JsonValue2> arrays)
        {
            if (value.Type == JsonValueType.Array)
            {
                foreach (var item in value.AsArray)
                {
                    if (item.Type == JsonValueType.String)
                    {
                        values.Add(item.ToString());
                    }
                }

                arrays.Add(value);
            }
        }
    }
}
