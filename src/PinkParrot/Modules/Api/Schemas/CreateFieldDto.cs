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

namespace PinkParrot.Modules.Api.Schemas
{
    public class CreateFieldDto
    {
        [Required]
        [JsonProperty("$type")]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public JToken Properties { get; set; }
    }
}
