// ==========================================================================
//  UpdateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class UpdateFieldDto
    {
        [Required]
        public JObject Properties { get; set; }
    }
}
