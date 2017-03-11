// ==========================================================================
//  FieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Controllers.Api.Schemas.Models.Converters;
using Squidex.Core.Schemas;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "fieldType")]
    [KnownType(typeof(BooleanFieldPropertiesDto))]
    [KnownType(typeof(DateTimeFieldPropertiesDto))]
    [KnownType(typeof(GeolocationFieldPropertiesDto))]
    [KnownType(typeof(JsonFieldPropertiesDto))]
    [KnownType(typeof(NumberFieldPropertiesDto))]
    [KnownType(typeof(StringFieldPropertiesDto))]
    public abstract class FieldPropertiesDto
    {
        /// <summary>
        /// Optional label for the editor.
        /// </summary>
        [StringLength(100)]
        public string Label { get; set; }

        /// <summary>
        /// Hints to describe the schema.
        /// </summary>
        [StringLength(1000)]
        public string Hints { get; set; }

        /// <summary>
        /// Placeholder to show when no value has been entered.
        /// </summary>
        [StringLength(100)]
        public string Placeholder { get; set; }

        /// <summary>
        /// Indicates if the field is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Determines if the field should be displayed in lists.
        /// </summary>
        public bool IsListField { get; set; }
        
        /// <summary>
        /// Determines if the field is localizable.
        /// </summary>
        public bool IsLocalizable { get; set; }

        public abstract FieldProperties ToProperties();
    }
}
