// ==========================================================================
//  JsonValueConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NJsonSchema.Infrastructure;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Geocoding;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<object>
    {
        private IGeocoder geocoder;

        public JToken Value { get; }

        private JsonValueConverter(JToken value, IGeocoder geocoder)
        {
            this.Value = value;
            this.geocoder = geocoder;
        }

        public static object ConvertValue(Field field, JToken json, IGeocoder geocoder)
        {
            return field.Accept(new JsonValueConverter(json, geocoder));
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
            List<string> addressString = new List<string>();
            var validProperties = new string[]
            {
                "latitude", "longitude", "address"
            };

            foreach (var property in geolocation.Properties())
            {
                if (!validProperties.Contains(property.Name.ToLower()))
                {
                    throw new InvalidCastException("Geolocation must have proper properties.");
                }
            }

            var lat = geolocation["latitude"];
            var lon = geolocation["longitude"];
            var address = geolocation["address"]?.ToString();

            if (lat == null || lon == null ||
                ((JValue)lat).Value == null || ((JValue)lon).Value == null)
            {
                var response = geocoder.GeocodeAddress(address);
                lat = response.TryGetPropertyValue("Latitude", (JToken)null);
                lon = response.TryGetPropertyValue("Longitude", (JToken)null);

                geolocation["latitude"] = lat;
                geolocation["longitude"] = lon;
            }

            if (!((double)lat).IsBetween(-90, 90))
            {
                throw new InvalidCastException("Latitude must be between -90 and 90.");
            }

            if (!((double)lon).IsBetween(-180, 180))
            {
                throw new InvalidCastException("Longitude must be between -180 and 180.");
            }

            return geolocation;
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
