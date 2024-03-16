// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Roles = Squidex.Domain.Apps.Core.Apps.Role;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AssignContributorDto
    {
        /// <summary>
        /// The id or email of the user to add to the app.
        /// </summary>
        [LocalizedRequired]
        public string ContributorId { get; set; }

        /// <summary>
        /// The role of the contributor.
        /// </summary>
        public string? Role { get; set; } = Roles.Developer;

        /// <summary>
        /// Set to true to invite the user if he does not exist.
        /// </summary>
        public bool Invite { get; set; }

        public AssignContributor ToCommand()
        {
            return SimpleMapper.Map(this, new AssignContributor());
        }
    }
}