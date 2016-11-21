// ==========================================================================
//  AssignContributorDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Core.Apps;

namespace Squidex.Modules.Api.Apps.Models
{
    public class AssignContributorDto
    {
        public string ContributorId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PermissionLevel Permission { get; set; }
    }
}