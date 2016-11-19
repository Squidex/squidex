// ==========================================================================
//  ListAppDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Core.Apps;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class ListAppDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime LastModified { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PermissionLevel Permission { get; set; }
    }
}
