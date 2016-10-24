// ==========================================================================
//  UpdateFieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Modules.Api.Schemas.Models
{
    public class UpdateFieldDto
    {
        public JObject Properties { get; set; }
    }
}
