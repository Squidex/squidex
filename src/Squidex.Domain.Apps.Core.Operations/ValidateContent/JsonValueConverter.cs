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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<object>
    {
        private readonly IJsonValue value;

        private JsonValueConverter(IJsonValue value)
        {
            this.value = value;
        }

        public static object ConvertValue(IField field, IJsonValue json)
        {
            return field.Accept(new JsonValueConverter(json));
        }

        public object Visit(IArrayField field)
        {
            return ConvertToObjectList();
        }

        public object Visit(IField<AssetsFieldProperties> field)
        {
            return ConvertToGuidList();
        }

        public object Visit(IField<ReferencesFieldProperties> field)
        {
            return ConvertToGuidList();
        }

        public object Visit(IField<TagsFieldProperties> field)
        {
            return ConvertToStringList();
        }

        public object Visit(IField<BooleanFieldProperties> field)
        {
            if (value is JsonScalar<bool> b)
            {
                return b.Value;
            }

            throw new InvalidCastException("Invalid json type, expected boolean.");
        }

        public object Visit(IField<NumberFieldProperties> field)
        {
            if (value is JsonScalar<double> b)
            {
                return b.Value;
            }

            throw new InvalidCastException("Invalid json type, expected number.");
        }

        public object Visit(IField<StringFieldProperties> field)
        {
            if (value is JsonScalar<string> b)
            {
                return b.Value;
            }

            throw new InvalidCastException("Invalid json type, expected string.");
        }

        public object Visit(IField<DateTimeFieldProperties> field)
        {
            if (value.Type == JsonValueType.String)
            {
                var parseResult = InstantPattern.General.Parse(value.ToString());

                if (!parseResult.Success)
                {
                    throw parseResult.Exception;
                }

                return parseResult.Value;
            }

            throw new InvalidCastException("Invalid json type, expected string.");
        }

        public object Visit(IField<GeolocationFieldProperties> field)
        {
            if (value is JsonObject geolocation)
            {
                foreach (var propertyName in geolocation.Keys)
                {
                    if (!string.Equals(propertyName, "latitude", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(propertyName, "longitude", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidCastException("Geolocation can only have latitude and longitude property.");
                    }
                }

                if (geolocation.TryGetValue("latitude", out var latValue) && latValue is JsonScalar<double> latNumber)
                {
                    var lat = latNumber.Value;

                    if (!lat.IsBetween(-90, 90))
                    {
                        throw new InvalidCastException("Latitude must be between -90 and 90.");
                    }
                }
                else
                {
                    throw new InvalidCastException("Invalid json type, expected latitude/longitude object.");
                }

                if (geolocation.TryGetValue("longitude", out var lonValue) && lonValue is JsonScalar<double> lonNumber)
                {
                    var lon = lonNumber.Value;

                    if (!lon.IsBetween(-180, 180))
                    {
                        throw new InvalidCastException("Longitude must be between -180 and 180.");
                    }
                }
                else
                {
                    throw new InvalidCastException("Invalid json type, expected latitude/longitude object.");
                }

                return value;
            }

            throw new InvalidCastException("Invalid json type, expected latitude/longitude object.");
        }

        public object Visit(IField<JsonFieldProperties> field)
        {
            return value;
        }

        private object ConvertToGuidList()
        {
            if (value is JsonArray array)
            {
                var result = new List<Guid>();

                foreach (var item in array)
                {
                    if (item is JsonScalar<string> s && Guid.TryParse(s.Value, out var guid))
                    {
                        result.Add(guid);
                    }
                    else
                    {
                        throw new InvalidCastException("Invalid json type, expected array of guid strings.");
                    }
                }

                return result;
            }

            throw new InvalidCastException("Invalid json type, expected array of guid strings.");
        }

        private object ConvertToStringList()
        {
            if (value is JsonArray array)
            {
                var result = new List<string>();

                foreach (var item in array)
                {
                    if (item is JsonNull n)
                    {
                        result.Add(null);
                    }
                    else if (item is JsonScalar<string> s)
                    {
                        result.Add(s.Value);
                    }
                    else
                    {
                        throw new InvalidCastException("Invalid json type, expected array of strings.");
                    }
                }

                return result;
            }

            throw new InvalidCastException("Invalid json type, expected array of strings.");
        }

        private object ConvertToObjectList()
        {
            if (value is JsonArray array)
            {
                var result = new List<JsonObject>();

                foreach (var item in array)
                {
                    if (item is JsonObject obj)
                    {
                        result.Add(obj);
                    }
                    else
                    {
                        throw new InvalidCastException("Invalid json type, expected array of objects.");
                    }
                }

                return result;
            }

            throw new InvalidCastException("Invalid json type, expected array of objects.");
        }
    }
}
