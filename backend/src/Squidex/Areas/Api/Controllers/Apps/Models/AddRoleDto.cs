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
    public sealed class AddRoleDto
    {
        /// <summary>
        /// The role name.
        /// </summary>
        [LocalizedRequired]
        public string Name { get; set; }

        public AddRole ToCommand()
        {
            return new AddRole { Name = Name };
        }
    }
}
