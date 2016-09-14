// ==========================================================================
//  EntityCreatedDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PinkParrot.Modules.Api
{
    public class EntityCreatedDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
