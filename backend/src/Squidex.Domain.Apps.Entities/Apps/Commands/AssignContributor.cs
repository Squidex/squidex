// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Roles = Squidex.Domain.Apps.Core.Apps.Role;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AssignContributor : AppUpdateCommand
    {
        public string ContributorId { get; set; }

        public string Role { get; set; } = Roles.Editor;

        public bool Restoring { get; set; }

        public bool Invite { get; set; }
    }
}