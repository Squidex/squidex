// ==========================================================================
//  AddFieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Schemas.Models
{
    public sealed class AddFieldDto
    {
        /// <summary>
        /// The name of the field. Must be unique within the schema.
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Determines the optional partitioning of the field.
        /// </summary>
        public string Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }
    }
}