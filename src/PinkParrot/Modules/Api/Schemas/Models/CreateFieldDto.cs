// ==========================================================================
//  CreateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class CreateFieldDto
    {
        [Required]
        [JsonProperty("$type")]
        public string Type { get; set; }

        [Required]
        public string Name { get; set; }
        
        public JObject Properties { get; set; }
    }
}
