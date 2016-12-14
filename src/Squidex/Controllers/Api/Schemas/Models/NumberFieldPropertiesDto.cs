// ==========================================================================
//  NumberFieldPropertiesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NJsonSchema.Annotations;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("Number")]
    public sealed class NumberFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The default value for the field value.
        /// </summary>
        public double? DefaultValue { get; set; }

        /// <summary>
        /// The maximum allowed value for the field value.
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// The minimum allowed value for the field value.
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// The allowed values for the field value.
        /// </summary>
        public double[] AllowedValues { get; set; }

        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new NumberFieldProperties());
        }
    }
}
