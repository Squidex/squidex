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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate IJsonValue? ValueConverter(IJsonValue value, IField field, IArrayField? parent = null);

    public static class ValueConverters
    {
        public static readonly ValueConverter Noop = (value, field, parent) => value;

        public static readonly ValueConverter ExcludeHidden = (value, field, parent) =>
        {
            return field.IsForApi() ? value : null;
        };

        public static readonly ValueConverter ExcludeChangedTypes = (value, field, parent) =>
        {
            if (value.Type == JsonValueType.Null)
            {
                return value;
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

            return value;
        };

        public static ValueConverter DecodeJson(IJsonSerializer jsonSerializer)
        {
            return (value, field, parent) =>
            {
                if (field is IField<JsonFieldProperties> && value is JsonString s)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(s.Value));

                    return jsonSerializer.Deserialize<IJsonValue>(decoded);
                }

                return value;
            };
        }

        public static ValueConverter EncodeJson(IJsonSerializer jsonSerializer)
        {
            return (value, field, parent) =>
            {
                if (value.Type != JsonValueType.Null && field is IField<JsonFieldProperties>)
                {
                    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonSerializer.Serialize(value)));

                    return JsonValue.Create(encoded);
                }

                return value;
            };
        }

        public static ValueConverter ResolveAssetUrls(NamedId<DomainId> appId, IReadOnlyCollection<string>? fields, IUrlGenerator urlGenerator)
        {
            if (fields?.Any() != true)
            {
                return Noop;
            }

            Func<IField, IField?, bool> shouldHandle;

            if (fields.Contains("*"))
            {
                shouldHandle = (field, parent) => true;
            }
            else
            {
                var paths = fields.Select(x => x.Split('.')).ToList();

                shouldHandle = (field, parent) =>
                {
                    for (var i = 0; i < paths.Count; i++)
                    {
                        var path = paths[i];

                        if (parent != null)
                        {
                            if (path.Length == 2 && path[0] == parent.Name && path[1] == field.Name)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (path.Length == 1 && path[0] == field.Name)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                };
            }

            return (value, field, parent) =>
            {
                if (field is IField<AssetsFieldProperties> && value is JsonArray array && shouldHandle(field, parent))
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        var id = array[i].ToString();

                        array[i] = JsonValue.Create(urlGenerator.AssetContent(appId, id));
                    }
                }

                return value;
            };
        }

        public static ValueConverter ForNested(params ValueConverter[] converters)
        {
            if (converters?.Any() != true)
            {
                return Noop;
            }

            return (value, field, parent) =>
            {
                if (value is JsonArray array && field is IArrayField arrayField)
                {
                    foreach (var nested in array.OfType<JsonObject>())
                    {
                        foreach (var (fieldName, nestedValue) in nested.ToList())
                        {
                            IJsonValue? newValue = nestedValue;

                            if (arrayField.FieldsByName.TryGetValue(fieldName, out var nestedField))
                            {
                                for (var i = 0; i < converters.Length; i++)
                                {
                                    newValue = converters[i](newValue!, nestedField, arrayField);

                                    if (newValue == null)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                newValue = null;
                            }

                            if (newValue == null)
                            {
                                nested.Remove(fieldName);
                            }
                            else if (!ReferenceEquals(nestedValue, newValue))
                            {
                                nested[fieldName] = newValue;
                            }
                        }
                    }
                }

                return value;
            };
        }
    }
}
