// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Tags
{
    public static class TagNormalizer
    {
        public static async Task NormalizeAsync(this ITagService tagService, Guid appId, Guid schemaId, Schema schema, NamedContentData newData, NamedContentData oldData)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(newData, nameof(newData));

            var newValues = new HashSet<string>();
            var newArrays = new List<JArray>();

            var oldValues = new HashSet<string>();
            var oldArrays = new List<JArray>();

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
                            array[i] = result;
                        }
                    }
                }
            }
        }

        public static async Task DenormalizeAsync(this ITagService tagService, Guid appId, Guid schemaId, Schema schema, params NamedContentData[] datas)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(schema, nameof(schema));

            var tagsValues = new HashSet<string>();
            var tagsArrays = new List<JArray>();

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
                            array[i] = result;
                        }
                    }
                }
            }
        }

        private static void GetValues(Schema schema, HashSet<string> values, List<JArray> arrays, params NamedContentData[] datas)
        {
            foreach (var field in schema.Fields)
            {
                if (field is IField<TagsFieldProperties> tags && tags.Properties.Normalization == TagsFieldNormalization.Schema)
                {
                    foreach (var data in datas)
                    {
                        if (data.TryGetValue(field.Name, out var fieldData))
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
                                if (data.TryGetValue(field.Name, out var fieldData))
                                {
                                    foreach (var partition in fieldData)
                                    {
                                        if (partition.Value is JArray jArray)
                                        {
                                            foreach (var value in jArray)
                                            {
                                                if (value.Type == JTokenType.Object)
                                                {
                                                    var nestedObject = (JObject)value;

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

        private static void ExtractTags(JToken token, ISet<string> values, ICollection<JArray> arrays)
        {
            if (token is JArray jArray)
            {
                foreach (var value in jArray)
                {
                    if (value.Type == JTokenType.String)
                    {
                        values.Add(value.ToString());
                    }
                }

                arrays.Add(jArray);
            }
        }
    }
}
