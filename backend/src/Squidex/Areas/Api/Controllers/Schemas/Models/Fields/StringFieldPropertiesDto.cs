﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class StringFieldPropertiesDto : FieldPropertiesDto
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
        public ReadOnlyCollection<string> AllowedValues { get; set; }

        /// <summary>
        /// Indicates if the field value must be unique. Ignored for nested fields and localized fields.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Indicates that the inline editor is enabled for this field.
        /// </summary>
        public bool InlineEditable { get; set; }

        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        public StringFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new StringFieldProperties());

            return result;
        }
    }
}
