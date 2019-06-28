// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowTransitionDto
    {
        /// <summary>
        /// The optional expression.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// The optional restricted role.
        /// </summary>
        public string Role { get; set; }
    }
}
