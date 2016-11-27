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
using NJsonSchema.Converters;
using Squidex.Core.Schemas;
using Dtos = Squidex.Controllers.Api.Schemas.Models.Fields;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "fieldType")]
    [KnownType(typeof(Dtos.NumberField))]
    [KnownType(typeof(Dtos.StringField))]
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

        public abstract FieldProperties ToProperties();
    }
}
