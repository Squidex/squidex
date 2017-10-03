// ==========================================================================
//  GeolocationField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class GeolocationField : Field<GeolocationFieldProperties>
    {
        public GeolocationField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new GeolocationFieldProperties())
        {
        }

        public GeolocationField(long id, string name, Partitioning partitioning, GeolocationFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public override object ConvertValue(JToken value)
        {
            var geolocation = (JObject)value;

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

            Guard.Between(lat,  -90,  90, "latitude");
            Guard.Between(lon, -180, 180, "longitude");

            return value;
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Object;

            var geolocationSchema = new JsonSchema4();

            geolocationSchema.Properties.Add("latitude", new JsonProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -90,
                Maximum = 90,
                IsRequired = true
            });
            geolocationSchema.Properties.Add("longitude", new JsonProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -180,
                Maximum = 180,
                IsRequired = true
            });

            geolocationSchema.AllowAdditionalProperties = false;

            var schemaReference = schemaResolver("GeolocationDto", geolocationSchema);

            jsonProperty.Reference = schemaReference;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return null;
        }
    }
}
