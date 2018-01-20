// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<object>
    {
        public JToken Value { get; }

        private JsonValueConverter(JToken value)
        {
            this.Value = value;
        }

        public static object ConvertValue(Field field, JToken json)
        {
            return field.Accept(new JsonValueConverter(json));
        }

        public object Visit(AssetsField field)
        {
            return Value.ToObject<List<Guid>>();
        }

        public object Visit(BooleanField field)
        {
            return (bool?)Value;
        }

        public object Visit(DateTimeField field)
        {
            if (Value.Type == JTokenType.String)
            {
                var parseResult = InstantPattern.General.Parse(Value.ToString());

                if (!parseResult.Success)
                {
                    throw parseResult.Exception;
                }

                return parseResult.Value;
            }

            throw new InvalidCastException("Invalid json type, expected string.");
        }

        public object Visit(GeolocationField field)
        {
            var geolocation = (JObject)Value;

            foreach (var property in geolocation.Properties())
            {
                if (!string.Equals(property.Name, "latitude", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(property.Name, "longitude", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidCastException("Geolocation can only have latitude and longitude property.");
                }
            }

            var lat = (double)geolocation["latitude"];
            var lon = (double)geolocation["longitude"];

            if (!lat.IsBetween(-90, 90))
            {
                throw new InvalidCastException("Latitude must be between -90 and 90.");
            }

            if (!lon.IsBetween(-180, 180))
            {
                throw new InvalidCastException("Longitude must be between -180 and 180.");
            }

            return Value;
        }

        public object Visit(JsonField field)
        {
            return Value;
        }

        public object Visit(NumberField field)
        {
            return (double?)Value;
        }

        public object Visit(ReferencesField field)
        {
            return Value.ToObject<List<Guid>>();
        }

        public object Visit(StringField field)
        {
            return Value.ToString();
        }

        public object Visit(TagsField field)
        {
            return Value.ToObject<List<string>>();
        }
    }
}
