// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate JToken ValueConverter(JToken value, IField field);

    public static class ValueConverters
    {
        public static ValueConverter DecodeJson()
        {
            return (value, field) =>
            {
                if (!value.IsNull() && field is IField<JsonFieldProperties>)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value.ToString()));

                   return JToken.Parse(decoded);
                }

                return value;
            };
        }

        public static ValueConverter EncodeJson()
        {
            return (value, field) =>
            {
                if (!value.IsNull() && field is IField<JsonFieldProperties>)
                {
                    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString()));

                    return encoded;
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
                if (value.IsNull())
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
