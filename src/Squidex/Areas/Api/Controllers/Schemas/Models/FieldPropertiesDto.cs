// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Areas.Api.Controllers.Schemas.Models.Fields;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "fieldType")]
    [KnownType(typeof(AssetsFieldPropertiesDto))]
    [KnownType(typeof(BooleanFieldPropertiesDto))]
    [KnownType(typeof(DateTimeFieldPropertiesDto))]
    [KnownType(typeof(GeolocationFieldPropertiesDto))]
    [KnownType(typeof(JsonFieldPropertiesDto))]
    [KnownType(typeof(NumberFieldPropertiesDto))]
    [KnownType(typeof(ReferencesFieldPropertiesDto))]
    [KnownType(typeof(StringFieldPropertiesDto))]
    [KnownType(typeof(TagsFieldPropertiesDto))]
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
        /// Gets the partitioning of the language, e.g. invariant or language.
        /// </summary>
        public string Partitioning { get; set; }

        public abstract FieldProperties ToProperties();
    }
}
