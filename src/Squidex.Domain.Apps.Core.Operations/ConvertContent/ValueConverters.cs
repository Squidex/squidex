// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate IJsonValue ValueConverter(IJsonValue value, IField field);

    public static class ValueConverters
    {
        public static ValueConverter DecodeJson(IJsonSerializer jsonSerializer)
        {
            return (value, field) =>
            {
                if (field is IField<JsonFieldProperties> && value is JsonScalar<string> s)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(s.Value));

                    return jsonSerializer.Deserialize<IJsonValue>(decoded);
                }

                return value;
            };
        }

        public static ValueConverter EncodeJson(IJsonSerializer jsonSerializer)
        {
            return (value, field) =>
            {
                if (value.Type != JsonValueType.Null && field is IField<JsonFieldProperties>)
                {
                    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonSerializer.Serialize(value)));

                    return JsonValue.Create(encoded);
                }

                return value;
            };
        }

        public static ValueConverter ExcludeHidden()
        {
            return (value, field) => field.IsHidden ? Value.Unset : value;
        }

        public static ValueConverter ExcludeChangedTypes()
        {
            return (value, field) =>
            {
                if (value.Type == JsonValueType.Null)
                {
                    return value;
                }

                try
                {
                    JsonValueConverter.ConvertValue(field, value);
                }
                catch
                {
                    return Value.Unset;
                }

                return value;
            };
        }
    }
}
