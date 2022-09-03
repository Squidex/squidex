// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Roles = Squidex.Domain.Apps.Core.Apps.Role;

namespace Squidex.Domain.Apps.Entities.Teams.Commands
{
    public sealed class AssignContributor : TeamUpdateCommand
    {
        public string ContributorId { get; set; }

        public string Role { get; set; } = Roles.Editor;

        public bool IgnoreActor { get; set; }

        public bool IgnorePlans { get; set; }

        public bool Invite { get; set; }
    }
}
