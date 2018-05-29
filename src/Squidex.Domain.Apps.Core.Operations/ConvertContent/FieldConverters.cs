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
    public static class FieldConverters
    {
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

        public static FieldConverter ResolveInvariant(LanguagesConfig languagesConfig)
        {
            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = languagesConfig.Master.Language.Iso2Code;

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

        public static FieldConverter ResolveLanguages(LanguagesConfig languagesConfig)
        {
            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = languagesConfig.Master.Language.Iso2Code;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    var result = new ContentFieldData();

                    foreach (var languageConfig in languagesConfig)
                    {
                        var languageCode = languageConfig.Key;

                        if (data.TryGetValue(languageCode, out var value))
                        {
                            result[languageCode] = value;
                        }
                        else if (languageConfig == languagesConfig.Master && data.TryGetValue(codeForInvariant, out value))
                        {
                            result[languageCode] = value;
                        }
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter ResolveFallbackLanguages(LanguagesConfig languagesConfig)
        {
            var master = languagesConfig.Master;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var languageConfig in languagesConfig)
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

        public static FieldConverter FilterLanguages(LanguagesConfig languagesConfig, IEnumerable<Language> languages)
        {
            if (languages == null)
            {
                return (data, field) => data;
            }

            var languageCodes = languages.Select(x => x.Iso2Code).Where(x => languagesConfig.Contains(x));
            var languageSet = new HashSet<string>(languageCodes, StringComparer.OrdinalIgnoreCase);

            if (languageSet.Count == 0)
            {
                return (data, field) => data;
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

        private static FieldConverter ForNested(params ValueConverter[] converters)
        {
            return (data, field) =>
            {
                if (field is IArrayField arrayField)
                {
                    foreach (var partition in data)
                    {
                        if (partition.Value is JArray jArray)
                        {
                            for (var i = 0; i < jArray.Count; i++)
                            {
                                if (jArray[i] is JObject item)
                                {
                                    var result = new JObject();

                                    foreach (var kvp in item)
                                    {
                                        if (!arrayField.FieldsByName.TryGetValue(kvp.Key, out var nestedField))
                                        {
                                            continue;
                                        }

                                        var newValue = kvp.Value;

                                        if (converters != null)
                                        {
                                            foreach (var converter in converters)
                                            {
                                                newValue = converter(newValue, field);

                                                if (ReferenceEquals(newValue, Value.Unset))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (!ReferenceEquals(newValue, Value.Unset))
                                        {
                                            result.Add(field.Id.ToString(), newValue);
                                        }
                                    }

                                    jArray[i] = item;
                                }
                            }
                        }
                    }
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
                    ContentFieldData result = null;

                    foreach (var partition in data)
                    {
                        var newValue = partition.Value;

                        if (converters != null)
                        {
                            foreach (var converter in converters)
                            {
                                newValue = converter(newValue, field);

                                if (ReferenceEquals(newValue, Value.Unset))
                                {
                                    break;
                                }
                            }
                        }

                        if (result != null || !ReferenceEquals(newValue, partition.Value))
                        {
                            if (result == null)
                            {
                                result = new ContentFieldData();
                            }

                            if (!ReferenceEquals(newValue, Value.Unset))
                            {
                                result.Add(partition.Key, newValue);
                            }
                        }
                    }

                    return result ?? data;
                }

                return data;
            };
        }
    }
}
