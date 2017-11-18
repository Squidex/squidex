// ==========================================================================
//  ChangePlanDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Plans.Models
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
