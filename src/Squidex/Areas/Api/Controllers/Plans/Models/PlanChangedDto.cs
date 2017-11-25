// ==========================================================================
//  PlanChangedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class PlanChangedDto
    {
        /// <summary>
        /// Optional redirect uri.
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
