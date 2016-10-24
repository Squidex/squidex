// ==========================================================================
//  CreateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class CreateFieldDto
    {
        [JsonProperty("$type")]
        public string Type { get; set; }
        
        public string Name { get; set; }
        
        public JObject Properties { get; set; }
    }
}
