// ==========================================================================
//  CreateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using PinkParrot.Core.Schema;

namespace PinkParrot.Modules.Api.Schemas
{
    public class CreateSchemaDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public ModelFieldProperties Properties { get; set; }
    }
}
