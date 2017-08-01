// ==========================================================================
//  PlanChangedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Plans.Models
{
    public class PlanChangedDto
    {
        /// <summary>
        /// Optional redirect uri.
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
