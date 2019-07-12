// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowStepDto
    {
        /// <summary>
        /// The transitions.
        /// </summary>
        [Required]
        public Dictionary<Status, WorkflowTransitionDto> Transitions { get; set; }

        /// <summary>
        /// The optional color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Indicates if updates should not be allowed.
        /// </summary>
        public bool NoUpdate { get; set; }
    }
}
