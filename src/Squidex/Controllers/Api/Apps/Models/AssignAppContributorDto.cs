// ==========================================================================
//  AssignAppContributorDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class AssignAppContributorDto
    {
        /// <summary>
        /// The id of the user to add to the app.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The permission level as a contributor.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AppContributorPermission Permission { get; set; }
    }
}