// ==========================================================================
//  DateTimeFieldPropertiesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Annotations;
using NodaTime;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("DateTime")]
    public sealed class DateTimeFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The default value for the field value.
        /// </summary>
        public Instant? DefaultValue { get; set; }

        /// <summary>
        /// The maximum allowed value for the field value.
        /// </summary>
        public Instant? MaxValue { get; set; }

        /// <summary>
        /// The minimum allowed value for the field value.
        /// </summary>
        public Instant? MinValue { get; set; }

        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DateTimeFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new DateTimeFieldProperties());

            return result;
        }
    }
}
