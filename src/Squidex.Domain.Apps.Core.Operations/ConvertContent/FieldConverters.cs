// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData FieldConverter(ContentFieldData data, IRootField field);

    public static class FieldConverters
    {
        private static readonly Func<IField, string> KeyNameResolver = f => f.Name;
        private static readonly Func<IField, string> KeyIdResolver = f => f.Id.ToString();
        private static readonly Func<IArrayField, string, IField> FieldByIdResolver =
            (f, k) => long.TryParse(k, out var id) ? f.FieldsById.GetOrDefault(id) : null;
        private static readonly Func<IArrayField, string, IField> FieldByNameResolver =
            (f, k) => f.FieldsByName.GetOrDefault(k);

        public static FieldConverter ExcludeHidden()
        {
            return (data, field) =>
            {
                return field.IsHidden ? null : data;
            };
        }

        public static FieldConverter ExcludeChangedTypes()
        {
            return (data, field) =>
            {
                foreach (var value in data.Values)
                {
                    if (value.IsNull())
                    {
                        continue;
                    }

                    try
                    {
                        JsonValueConverter.ConvertValue(field, value);
                    }
                    catch
                    {
                        return null;
                    }
                }

                return data;
            };
        }

        public static FieldConverter ResolveInvariant(LanguagesConfig config)
        {
            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = config.Master.Language.Iso2Code;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Invariant))
                {
                    var result = new ContentFieldData();

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

        public static FieldConverter ResolveLanguages(LanguagesConfig config)
        {
            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = config.Master.Language.Iso2Code;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    var result = new ContentFieldData();

                    foreach (var languageConfig in config)
                    {
                        var languageCode = languageConfig.Key;

                        if (data.TryGetValue(languageCode, out var value))
                        {
                            result[languageCode] = value;
                        }
                        else if (languageConfig == config.Master && data.TryGetValue(codeForInvariant, out value))
                        {
                            result[languageCode] = value;
                        }
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter ResolveFallbackLanguages(LanguagesConfig config)
        {
            var master = config.Master;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var languageConfig in config)
                    {
                        var languageCode = languageConfig.Key;

                        if (!data.TryGetValue(languageCode, out var value))
                        {
                            var dataFound = false;

                            foreach (var fallback in languageConfig.Fallback)
                            {
                                if (data.TryGetValue(fallback, out value))
                                {
                                    data[languageCode] = value;
                                    dataFound = true;
                                    break;
                                }
                            }

                            if (!dataFound && languageConfig != master)
                            {
                                if (data.TryGetValue(master.Language, out value))
                                {
                                    data[languageCode] = value;
                                }
                            }
                        }
                    }
                }

                return data;
            };
        }

        public static FieldConverter FilterLanguages(LanguagesConfig config, IEnumerable<Language> languages)
        {
            if (languages?.Any() != true)
            {
                return (data, field) => data;
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
                languageSet.Add(config.Master.Language.Iso2Code);
            }

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    var result = new ContentFieldData();

                    foreach (var languageCode in languageSet)
                    {
                        if (data.TryGetValue(languageCode, out var value))
                        {
                            result[languageCode] = value;
                        }
                    }

                    return result;
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

        private static FieldConverter ForNested(
            Func<IArrayField, string, IField> fieldResolver,
            Func<IField, string> keyResolver,
            params ValueConverter[] converters)
        {
            return (data, field) =>
            {
                if (field is IArrayField arrayField)
                {
                    var result = new ContentFieldData();

                    foreach (var partition in data)
                    {
                        if (!(partition.Value is JArray jArray))
                        {
                            continue;
                        }

                        var newArray = new JArray();

                        foreach (JObject item in jArray.OfType<JObject>())
                        {
                            var newItem = new JObject();

                            foreach (var kvp in item)
                            {
                                var nestedField = fieldResolver(arrayField, kvp.Key);

                                if (nestedField == null)
                                {
                                    continue;
                                }

                                var newValue = kvp.Value;

                                var isUnset = false;

                                if (converters != null)
                                {
                                    foreach (var converter in converters)
                                    {
                                        newValue = converter(newValue, nestedField);

                                        if (ReferenceEquals(newValue, Value.Unset))
                                        {
                                            isUnset = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isUnset)
                                {
                                    newItem.Add(keyResolver(nestedField), newValue);
                                }
                            }

                            newArray.Add(newItem);
                        }

                        result.Add(partition.Key, newArray);
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
                if (!(field is IArrayField))
                {
                    var result = new ContentFieldData();

                    foreach (var partition in data)
                    {
                        var newValue = partition.Value;

                        var isUnset = false;

                        if (converters != null)
                        {
                            foreach (var converter in converters)
                            {
                                newValue = converter(newValue, field);

                                if (ReferenceEquals(newValue, Value.Unset))
                                {
                                    isUnset = true;
                                    break;
                                }
                            }
                        }

                        if (!isUnset)
                        {
                            result.Add(partition.Key, newValue);
                        }
                    }

                    return result;
                }

                return data;
            };
        }
    }
}
