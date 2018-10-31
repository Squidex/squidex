// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AddRoleDto
    {
        /// <summary>
        /// The role name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public AddRole ToCommand()
        {
            return new AddRole { Name = Name };
        }
    }
}
