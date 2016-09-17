// ==========================================================================
//  UpdateSchemaDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using PinkParrot.Core.Schemas;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class UpdateSchemaDto
    {
        [Required]
        public SchemaProperties Properties { get; set; }
    }
}
