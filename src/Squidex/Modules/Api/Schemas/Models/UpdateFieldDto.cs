// ==========================================================================
//  UpdateFieldDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class UpdateFieldDto
    {
        public JObject Properties { get; set; }
    }
}
