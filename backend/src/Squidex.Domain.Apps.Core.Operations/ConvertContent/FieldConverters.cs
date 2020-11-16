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

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData? FieldConverter(ContentFieldData data, IRootField field);

    public static class FieldConverters
    {
        public static readonly FieldConverter Noop = (data, field) => data;

        public static readonly FieldConverter ExcludeHidden = (data, field) =>
        {
            return field.IsForApi() ? data : null;
        };

        public static readonly FieldConverter ExcludeChangedTypes = (data, field) =>
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

        public static FieldConverter ResolveInvariant(LanguagesConfig languages)
        {
            var codeForInvariant = InvariantPartitioning.Key;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Invariant) && !data.ContainsKey(codeForInvariant))
                {
                    var result = new ContentFieldData(1);

                    if (data.TryGetValue(languages.Master, out var value))
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
                    if (data.TryGetValue(codeForInvariant, out var value))
                    {
                        var result = new ContentFieldData
                        {
                            [languages.Master] = value
                        };

                        return result;
                    }

                    foreach (var key in data.Keys.Where(x => !languages.AllKeys.Contains(x)).ToList())
                    {
                        data.Remove(key);
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

        public static FieldConverter ForValues(params ValueConverter[] converters)
        {
            return (data, field) =>
            {
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
