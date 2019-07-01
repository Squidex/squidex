// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AddWorkflowDto
    {
        /// <summary>
        /// The name of the workflow.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public ICommand ToCommand()
        {
            return new AddWorkflow { Name = Name };
        }
    }
}
