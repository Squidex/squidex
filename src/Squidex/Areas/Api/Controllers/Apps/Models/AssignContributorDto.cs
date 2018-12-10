// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AssignContributorDto
    {
        /// <summary>
        /// The id or email of the user to add to the app.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The role of the contributor.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Set to true to invite the user if he does not exist.
        /// </summary>
        public bool Invite { get; set; }

        public AssignContributor ToCommand()
        {
            return SimpleMapper.Map(this, new AssignContributor { IsInviting = Invite });
        }
    }
}