// ==========================================================================
//  CreateFieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Modules.Api.Schemas.Models
{
    public class CreateFieldDto
    {
        [JsonProperty("$type")]
        public string Type { get; set; }
        
        public string Name { get; set; }
        
        public JObject Properties { get; set; }
    }
}
