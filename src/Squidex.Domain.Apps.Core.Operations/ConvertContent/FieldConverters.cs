// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var isValid = true;

                foreach (var value in data.Values)
                {
                    try
                    {
                        if (!value.IsNull())
                        {
                            JsonValueConverter.ConvertValue(field, value);
                        }
                    }
                    catch
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                {
                    return null;
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

            var languageCodes =
                new HashSet<string>(
                        languages.Select(x => x.Iso2Code).Where(x => languagesConfig.Contains(x)),
                    StringComparer.OrdinalIgnoreCase);

            if (languageCodes.Count == 0)
            {
                return (data, field) => data;
            }

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    var result = new ContentFieldData();

                    foreach (var languageCode in languageCodes)
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

        public static FieldConverter DecodeJson()
        {
            return (data, field) =>
            {
                if (field is IField<JsonFieldProperties>)
                {
                    var result = new ContentFieldData();

                    foreach (var partitionValue in data)
                    {
                        if (partitionValue.Value.IsNull())
                        {
                            result[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Encoding.UTF8.GetString(Convert.FromBase64String(partitionValue.Value.ToString()));

                            result[partitionValue.Key] = JToken.Parse(value);
                        }
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter EncodeJson()
        {
            return (data, field) =>
            {
                if (field is IField<JsonFieldProperties>)
                {
                    var result = new ContentFieldData();

                    foreach (var partitionValue in data)
                    {
                        if (partitionValue.Value.IsNull())
                        {
                            result[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(partitionValue.Value.ToString()));

                            result[partitionValue.Key] = value;
                        }
                    }

                    return result;
                }

                return data;
            };
        }
    }
}
