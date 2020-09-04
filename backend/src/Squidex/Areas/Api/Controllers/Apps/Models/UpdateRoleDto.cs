// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateRoleDto
    {
        /// <summary>
        /// Associated list of permissions.
        /// </summary>
        [LocalizedRequired]
        public string[] Permissions { get; set; }

        public UpdateRole ToCommand(string name)
        {
            return new UpdateRole { Name = name, Permissions = Permissions };
        }
    }
}
