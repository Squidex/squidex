// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData? FieldConverter(ContentFieldData data, IRootField field);

    public static class FieldConverters
    {
        private delegate string FieldKeyResolver(IField field);

        private static readonly FieldKeyResolver KeyNameResolver = f => f.Name;
        private static readonly FieldKeyResolver KeyIdResolver = f => f.Id.ToString();

        public static readonly FieldConverter Noop = (data, field) => data;

        private delegate IField? FieldResolver(IArrayField field, string key);

        private static IField? FieldByIdResolver(IArrayField array, string key)
        {
            if (key != null && long.TryParse(key, out var id))
            {
                return array.FieldsById.GetOrDefault(id);
            }

            return null;
        }

        private static IField? FieldByNameResolver(IArrayField array, string key)
        {
            if (key != null)
            {
                return array.FieldsByName.GetOrDefault(key);
            }

            return null;
        }

        public static FieldConverter ExcludeHidden()
        {
            return (data, field) => field.IsForApi() ? data : null;
        }

        public static FieldConverter ExcludeChangedTypes()
        {
            return (data, field) =>
            {
                foreach (var value in data.Values)
                {
                    if (value.Type == JsonValueType.Null)
                    {
                        continue;
                    }

                    try
                    {
                        var (_, error) = JsonValueConverter.ConvertValue(field, value);

                        if (error != null)
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }

                return data;
            };
        }

        public static FieldConverter ResolveInvariant(LanguagesConfig languages)
        {
            var codeForInvariant = InvariantPartitioning.Key;
            var codeForMasterLanguage = languages.Master;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Invariant) && (!data.ContainsKey(codeForInvariant) || data.Count != 1))
                {
                    var result = new ContentFieldData(1);

                    if (data.TryGetValue(codeForInvariant, out var value))
                    {
                        result[codeForInvariant] = value;
                    }
                    else if (data.TryGetValue(codeForMasterLanguage, out value))
                    {
                        result[codeForInvariant] = value;
                    }
                    else if (data.Count > 0)
                    {
                        result[codeForInvariant] = data.Values.First();
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter ResolveLanguages(LanguagesConfig languages)
        {
            var codeForInvariant = InvariantPartitioning.Key;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var languageCode in languages.AllKeys)
                    {
                        if (!data.ContainsKey(languageCode))
                        {
                            if (data.TryGetValue(languageCode, out var value))
                            {
                                data[languageCode] = value;
                            }
                            else if (languages.IsMaster(languageCode) && data.TryGetValue(codeForInvariant, out value))
                            {
                                data[languageCode] = value;
                            }
                        }
                    }
                }

                return data;
            };
        }

        public static FieldConverter ResolveFallbackLanguages(LanguagesConfig languages)
        {
            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var languageCode in languages.AllKeys)
                    {
                        if (!data.ContainsKey(languageCode))
                        {
                            foreach (var fallback in languages.GetPriorities(languageCode))
                            {
                                if (data.TryGetValue(fallback, out var value))
                                {
                                    data[languageCode] = value;
                                    break;
                                }
                            }
                        }
                    }
                }

                return data;
            };
        }

        public static FieldConverter FilterLanguages(LanguagesConfig config, IEnumerable<Language>? languages)
        {
            if (languages?.Any() != true)
            {
                return Noop;
            }

            var languageSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var language in languages)
            {
                if (config.Contains(language.Iso2Code))
                {
                    languageSet.Add(language.Iso2Code);
                }
            }

            if (languageSet.Count == 0)
            {
                languageSet.Add(config.Master);
            }

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var (key, _) in data.ToList())
                    {
                        if (!languageSet.Contains(key))
                        {
                            data.Remove(key);
                        }
                    }
                }

                return data;
            };
        }

        public static FieldConverter ForNestedName2Name(params ValueConverter[] converters)
        {
            return ForNested(FieldByNameResolver, KeyNameResolver, converters);
        }

        public static FieldConverter ForNestedName2Id(params ValueConverter[] converters)
        {
            return ForNested(FieldByNameResolver, KeyIdResolver, converters);
        }

        public static FieldConverter ForNestedId2Name(params ValueConverter[] converters)
        {
            return ForNested(FieldByIdResolver, KeyNameResolver, converters);
        }

        public static FieldConverter ForNestedId2Id(params ValueConverter[] converters)
        {
            return ForNested(FieldByIdResolver, KeyIdResolver, converters);
        }

        private static FieldConverter ForNested(FieldResolver fieldResolver, FieldKeyResolver keyResolver, params ValueConverter[] converters)
        {
            return (data, field) =>
            {
                if (field is IArrayField arrayField)
                {
                    var result = new ContentFieldData();

                    foreach (var (partitionKey, partitionValue) in data)
                    {
                        if (!(partitionValue is JsonArray array))
                        {
                            continue;
                        }

                        var newArray = JsonValue.Array();

                        foreach (var item in array.OfType<JsonObject>())
                        {
                            var newItem = JsonValue.Object();

                            foreach (var (key, value) in item)
                            {
                                var nestedField = fieldResolver(arrayField, key);

                                if (nestedField == null)
                                {
                                    continue;
                                }

                                IJsonValue? newValue = value;

                                if (converters != null)
                                {
                                    for (var i = 0; i < converters.Length; i++)
                                    {
                                        newValue = converters[i](newValue!, nestedField, arrayField);

                                        if (value == null)
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (newValue != null)
                                {
                                    newItem.Add(keyResolver(nestedField), newValue);
                                }
                            }

                            newArray.Add(newItem);
                        }

                        result.Add(partitionKey, newArray);
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter ForValues(params ValueConverter[] converters)
        {
            return (data, field) =>
            {
                if (field is IArrayField arrayField)
                {
                    return data;
                }

                foreach (var (key, value) in data.ToList())
                {
                    IJsonValue? newValue = value;

                    for (var i = 0; i < converters.Length; i++)
                    {
                        newValue = converters[i](newValue!, field, null);

                        if (newValue == null)
                        {
                            break;
                        }
                    }

                    if (newValue == null)
                    {
                        data.Remove(key);
                    }
                    else if (!ReferenceEquals(newValue, value))
                    {
                        data[key] = newValue;
                    }
                }

                return data;
            };
        }
    }
}
