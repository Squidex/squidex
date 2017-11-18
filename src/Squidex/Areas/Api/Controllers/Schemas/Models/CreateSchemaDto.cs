// ==========================================================================
//  CreateSchemaDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class CreateSchemaDto
    {
        /// <summary>
        /// The name of the schema.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The optional properties.
        /// </summary>
        public SchemaPropertiesDto Properties { get; set; }

        /// <summary>
        /// Optional fields.
        /// </summary>
        public List<CreateSchemaFieldDto> Fields { get; set; }
    }
}
