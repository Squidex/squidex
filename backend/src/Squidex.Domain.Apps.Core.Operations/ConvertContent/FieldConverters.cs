// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate ContentFieldData? FieldConverter(ContentFieldData data, IRootField field);

    public static class FieldConverters
    {
        public static readonly FieldConverter Noop = (data, _) => data;

        public static readonly FieldConverter ExcludeHidden = (data, field) =>
        {
            return field.IsForApi() ? data : null;
        };

        public static FieldConverter ExcludeChangedTypes(IJsonSerializer jsonSerializer)
        {
            return (data, field) =>
            {
                foreach (var (_, value) in data)
                {
                    if (value.Type == JsonValueType.Null)
                    {
                        continue;
                    }

                    try
                    {
                        if (!JsonValueValidator.IsValid(field, value, jsonSerializer))
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
            var iv = InvariantPartitioning.Key;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Invariant) && !data.TryGetNonNull(iv, out _))
                {
                    var result = new ContentFieldData(1);

                    if (data.TryGetNonNull(languages.Master, out var value))
                    {
                        result[iv] = value;
                    }
                    else if (data.Count > 0)
                    {
                        result[iv] = data.First().Value;
                    }

                    return result;
                }

                return data;
            };
        }

        public static FieldConverter ResolveLanguages(LanguagesConfig languages)
        {
            var iv = InvariantPartitioning.Key;

            return (data, field) =>
            {
                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    if (data.TryGetNonNull(iv, out var value))
                    {
                        var result = new ContentFieldData
                        {
                            [languages.Master] = value
                        };

                        return result;
                    }

                    while (true)
                    {
                        var isRemoved = false;

                        foreach (var (key, _) in data)
                        {
                            if (!languages.AllKeys.Contains(key))
                            {
                                data.Remove(key);
                                isRemoved = true;
                                break;
                            }
                        }

                        if (!isRemoved)
                        {
                            break;
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
                        if (data.TryGetNonNull(languageCode, out _))
                        {
                            continue;
                        }

                        foreach (var fallback in languages.GetPriorities(languageCode))
                        {
                            if (data.TryGetNonNull(fallback, out var value))
                            {
                                data[languageCode] = value;
                                break;
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
                    while (true)
                    {
                        var isRemoved = false;

                        foreach (var (key, _) in data)
                        {
                            if (!languageSet.Contains(key))
                            {
                                data.Remove(key);
                                isRemoved = true;
                                break;
                            }
                        }

                        if (!isRemoved)
                        {
                            break;
                        }
                    }
                }

                return data;
            };
        }

        public static FieldConverter ForValues(ResolvedComponents components, params ValueConverter[] converters)
        {
            return (data, field) =>
            {
                ContentFieldData? newData = null;

                foreach (var (key, value) in data)
                {
                    var newValue = ConvertByType(field, value, null, converters, components);

                    if (newValue == null)
                    {
                        newData ??= new ContentFieldData(data);
                        newData.Remove(key);
                    }
                    else if (!ReferenceEquals(newValue.Value.RawValue, value.RawValue))
                    {
                        newData ??= new ContentFieldData(data);
                        newData[key] = newValue.Value;
                    }
                }

                return newData ?? data;
            };
        }

        private static JsonValue? ConvertByType<T>(T field, JsonValue value, IArrayField? parent, ValueConverter[] converters,
            ResolvedComponents components) where T : IField
        {
            switch (field)
            {
                case IArrayField arrayField:
                    return ConvertArray(arrayField, value, converters, components);

                case IField<ComponentFieldProperties>:
                    return ConvertComponent(value, converters, components);

                case IField<ComponentsFieldProperties>:
                    return ConvertComponents(value, converters, components);

                default:
                    return ConvertValue(field, value, parent, converters);
            }
        }

        private static JsonValue? ConvertArray(IArrayField field, JsonValue value, ValueConverter[] converters,
            ResolvedComponents components)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                JsonArray? result = null;

                for (int i = 0, j = 0; i < array.Count; i++, j++)
                {
                    var oldValue = array[i];

                    var newValue = ConvertArrayItem(field, oldValue, converters, components);

                    if (newValue == null)
                    {
                        result ??= new JsonArray(array);
                        result.RemoveAt(j);
                        j--;
                    }
                    else if (!ReferenceEquals(newValue.Value.RawValue, oldValue.RawValue))
                    {
                        result ??= new JsonArray(array);
                        result[j] = newValue.Value;
                    }
                }

                return result ?? value;
            }

            return null;
        }

        private static JsonValue? ConvertComponents(JsonValue? value, ValueConverter[] converters,
            ResolvedComponents components)
        {
            if (value?.Type == JsonValueType.Array)
            {
                var array = value.Value.AsArray;

                JsonArray? result = null;

                for (int i = 0, j = 0; i < array.Count; i++, j++)
                {
                    var oldValue = array[i];

                    var newValue = ConvertComponent(oldValue, converters, components);

                    if (newValue == null)
                    {
                        result ??= new JsonArray(array);
                        result.RemoveAt(j);
                        j--;
                    }
                    else if (!ReferenceEquals(newValue.Value.RawValue, array[i].RawValue))
                    {
                        result ??= new JsonArray(array);
                        result[j] = newValue.Value;
                    }
                }

                return result ?? value;
            }

            return null;
        }

        private static JsonValue? ConvertComponent(JsonValue? value, ValueConverter[] converters,
            ResolvedComponents components)
        {
            if (value.HasValue && value.Value.Type == JsonValueType.Object && value.Value.AsObject.TryGetValue(Component.Discriminator, out var type) && type.Type == JsonValueType.String)
            {
                var id = DomainId.Create(type.AsString);

                if (components.TryGetValue(id, out var schema))
                {
                    return ConvertNested(schema.FieldCollection, value.Value, null, converters, components);
                }
                else
                {
                    return value;
                }
            }

            return null;
        }

        private static JsonValue? ConvertArrayItem(IArrayField field, JsonValue value, ValueConverter[] converters,
            ResolvedComponents components)
        {
            if (value.Type == JsonValueType.Object)
            {
                return ConvertNested(field.FieldCollection, value, field, converters, components);
            }

            return null;
        }

        private static JsonValue ConvertNested<T>(FieldCollection<T> fields, JsonValue source, IArrayField? parent, ValueConverter[] converters,
            ResolvedComponents components) where T : IField
        {
            JsonObject? result = null;

            var obj = source.AsObject;

            foreach (var (key, value) in obj)
            {
                JsonValue? newValue = value;

                if (fields.ByName.TryGetValue(key, out var field))
                {
                    newValue = ConvertByType(field, value, parent, converters, components);
                }
                else if (key != Component.Discriminator)
                {
                    newValue = null;
                }

                if (newValue == null)
                {
                    result ??= new JsonObject(obj);
                    result.Remove(key);
                }
                else if (!ReferenceEquals(newValue.Value.RawValue, value.RawValue))
                {
                    result ??= new JsonObject(obj);
                    result[key] = newValue.Value;
                }
            }

            return result ?? source;
        }

        private static JsonValue? ConvertValue(IField field, JsonValue value, IArrayField? parent, ValueConverter[] converters)
        {
            var newValue = value;

            for (var i = 0; i < converters.Length; i++)
            {
                var candidate = converters[i](newValue!, field, parent);

                if (candidate == null)
                {
                    return null;
                }
                else
                {
                    newValue = candidate.Value;
                }
            }

            return newValue;
        }
    }
}
