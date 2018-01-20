// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class ChangePlanDto
    {
        /// <summary>
        /// The new plan id.
        /// </summary>
        [Required]
        public string PlanId { get; set; }
    }
}
