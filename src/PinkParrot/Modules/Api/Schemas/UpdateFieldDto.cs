// ==========================================================================
//  UpdateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace PinkParrot.Modules.Api.Schemas
{
    public class UpdateFieldDto
    {
        [Required]
        public JToken Properties { get; set; }
    }
}
