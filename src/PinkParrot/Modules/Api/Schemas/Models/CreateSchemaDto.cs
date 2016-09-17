// ==========================================================================
//  CreateSchemaDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using PinkParrot.Core.Schemas;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class CreateSchemaDto
    {
        [Required]
        public string Name { get; set; }
        
        public FieldProperties Properties { get; set; }
    }
}
