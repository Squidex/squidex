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
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class AssignContributorDto
    {
        /// <summary>
        /// The id of the user to add to the app (GUID).
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The permission level as a contributor.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PermissionLevel Permission { get; set; }
    }
}