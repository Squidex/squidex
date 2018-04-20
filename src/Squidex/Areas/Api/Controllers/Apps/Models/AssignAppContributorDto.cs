// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AssignAppContributorDto
    {
        /// <summary>
        /// The id or email of the user to add to the app.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The permission level as a contributor.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AppContributorPermission Permission { get; set; }

        public AssignContributor ToCommand()
        {
            return SimpleMapper.Map(this, new AssignContributor());
        }
    }
}