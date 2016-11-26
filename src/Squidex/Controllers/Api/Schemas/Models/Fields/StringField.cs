// ==========================================================================
//  StringField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models.Fields
{
    public sealed class StringField : FieldDto
    {
        /// <summary>
        /// The default value for the field value.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// The pattern to enforce a specific format for the field value.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// The validation message for the pattern.
        /// </summary>
        public string PatternMessage { get; set; }

        /// <summary>
        /// The minimum allowed length for the field value.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// The maximum allowed length for the field value.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// The allowed values for the field value.
        /// </summary>
        public string[] AllowedValues { get; set; }

        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new StringFieldProperties());
        }
    }
}
