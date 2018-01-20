// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Annotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
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

        /// <summary>
        /// The calculated default value for the field value.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new DateTimeFieldProperties());

            return result;
        }
    }
}
