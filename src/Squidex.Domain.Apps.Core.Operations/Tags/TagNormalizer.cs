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

namespace Squidex.Domain.Apps.Core.Tags
{
    public static class TagNormalizer
    {
        public static async Task NormalizeAsync(ITagService service, Guid appId, Guid schemaId, Schema schema, params NamedContentData[] datas)
        {
            var tagsValues = new HashSet<string>();
            var tagsArrays = new List<JArray>();

            GetValues(schema, tagsValues, tagsArrays, datas);

            if (tagsValues.Count > 0)
            {
                var normalized = await service.NormalizeTagsAsync(appId, $"Schemas_{schemaId}", tagsValues, null);

                foreach (var array in tagsArrays)
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        array[i] = normalized[array[i].ToString()];
                    }
                }
            }
        }

        public static async Task DeNormalizeAsync(ITagService service, Guid appId, Guid schemaId, Schema schema, params NamedContentData[] datas)
        {
            var tagsValues = new HashSet<string>();
            var tagsArrays = new List<JArray>();

            GetValues(schema, tagsValues, tagsArrays, datas);

            if (tagsValues.Count > 0)
            {
                var denormalized = await service.DenormalizeTagsAsync(appId, $"Schemas_{schemaId}", tagsValues);

                foreach (var array in tagsArrays)
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        array[i] = denormalized[array[i].ToString()];
                    }
                }
            }
        }

        private static void GetValues(Schema schema, HashSet<string> values, List<JArray> arrays, params NamedContentData[] datas)
        {
            foreach (var field in schema.Fields)
            {
                if (field.RawProperties is TagsFieldProperties tags && tags.Normalize)
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
                        if (field.RawProperties is TagsFieldProperties nestedTags && nestedTags.Normalize)
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

                                                    if (nestedObject.TryGetValue(nestedField.Name, out _))
                                                    {
                                                        ExtractTags(partition.Value, values, arrays);
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
