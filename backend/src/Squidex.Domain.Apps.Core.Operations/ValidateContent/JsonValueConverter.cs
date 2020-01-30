// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<(object? Result, JsonError? Error)>
    {
        private readonly IJsonValue value;

        private JsonValueConverter(IJsonValue value)
        {
            this.value = value;
        }

        public static (object? Result, JsonError? Error) ConvertValue(IField field, IJsonValue json)
        {
            return field.Accept(new JsonValueConverter(json));
        }

        public (object? Result, JsonError? Error) Visit(IArrayField field)
        {
            return ConvertToObjectList();
        }

        public (object? Result, JsonError? Error) Visit(IField<AssetsFieldProperties> field)
        {
            return ConvertToGuidList();
        }

        public (object? Result, JsonError? Error) Visit(IField<ReferencesFieldProperties> field)
        {
            return ConvertToGuidList();
        }

        public (object? Result, JsonError? Error) Visit(IField<TagsFieldProperties> field)
        {
            return ConvertToStringList();
        }

        public (object? Result, JsonError? Error) Visit(IField<BooleanFieldProperties> field)
        {
            if (value is JsonScalar<bool> b)
            {
                return (b.Value, null);
            }

            return (null, new JsonError("Invalid json type, expected boolean."));
        }

        public (object? Result, JsonError? Error) Visit(IField<NumberFieldProperties> field)
        {
            if (value is JsonScalar<double> n)
            {
                return (n.Value, null);
            }

            return (null, new JsonError("Invalid json type, expected number."));
        }

        public (object? Result, JsonError? Error) Visit(IField<StringFieldProperties> field)
        {
            if (value is JsonScalar<string> s)
            {
                return (s.Value, null);
            }

            return (null, new JsonError("Invalid json type, expected string."));
        }

        public (object? Result, JsonError? Error) Visit(IField<UIFieldProperties> field)
        {
            return (value, null);
        }

        public (object? Result, JsonError? Error) Visit(IField<DateTimeFieldProperties> field)
        {
            if (value.Type == JsonValueType.String)
            {
                var parseResult = InstantPattern.General.Parse(value.ToString());

                if (!parseResult.Success)
                {
                    return (null, new JsonError(parseResult.Exception.Message));
                }

                return (parseResult.Value, null);
            }

            return (null, new JsonError("Invalid json type, expected string."));
        }

        public (object? Result, JsonError? Error) Visit(IField<GeolocationFieldProperties> field)
        {
            if (value is JsonObject geolocation)
            {
                foreach (var propertyName in geolocation.Keys)
                {
                    if (!string.Equals(propertyName, "latitude", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(propertyName, "longitude", StringComparison.OrdinalIgnoreCase))
                    {
                        return (null, new JsonError("Geolocation can only have latitude and longitude property."));
                    }
                }

                if (geolocation.TryGetValue("latitude", out var latValue) && latValue is JsonScalar<double> latNumber)
                {
                    var lat = latNumber.Value;

                    if (!lat.IsBetween(-90, 90))
                    {
                        return (null, new JsonError("Latitude must be between -90 and 90."));
                    }
                }
                else
                {
                    return (null, new JsonError("Invalid json type, expected latitude/longitude object."));
                }

                if (geolocation.TryGetValue("longitude", out var lonValue) && lonValue is JsonScalar<double> lonNumber)
                {
                    var lon = lonNumber.Value;

                    if (!lon.IsBetween(-180, 180))
                    {
                        return (null, new JsonError("Longitude must be between -180 and 180."));
                    }
                }
                else
                {
                    return (null, new JsonError("Invalid json type, expected latitude/longitude object."));
                }

                return (value, null);
            }

            return (null, new JsonError("Invalid json type, expected latitude/longitude object."));
        }

        public (object? Result, JsonError? Error) Visit(IField<JsonFieldProperties> field)
        {
            return (value, null);
        }

        private (object? Result, JsonError? Error) ConvertToGuidList()
        {
            if (value is JsonArray array)
            {
                var result = new List<Guid>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonScalar<string> s && Guid.TryParse(s.Value, out var guid))
                    {
                        result.Add(guid);
                    }
                    else
                    {
                        return (null, new JsonError("Invalid json type, expected array of guid strings."));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError("Invalid json type, expected array of guid strings."));
        }

        private (object? Result, JsonError? Error) ConvertToStringList()
        {
            if (value is JsonArray array)
            {
                var result = new List<string?>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonNull)
                    {
                        result.Add(null);
                    }
                    else if (item is JsonScalar<string> s)
                    {
                        result.Add(s.Value);
                    }
                    else
                    {
                        return (null, new JsonError("Invalid json type, expected array of strings."));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError("Invalid json type, expected array of strings."));
        }

        private (object? Result, JsonError? Error) ConvertToObjectList()
        {
            if (value is JsonArray array)
            {
                var result = new List<JsonObject>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonObject obj)
                    {
                        result.Add(obj);
                    }
                    else
                    {
                        return (null, new JsonError("Invalid json type, expected array of objects."));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError("Invalid json type, expected array of objects."));
        }
    }
}
