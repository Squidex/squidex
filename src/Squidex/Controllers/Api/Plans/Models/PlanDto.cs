// ==========================================================================
//  PlanDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Plans.Models
{
    public sealed class PlanDto
    {
        /// <summary>
        /// The id of the plan.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the plan.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The monthly costs of the plan.
        /// </summary>
        public string Costs { get; set; }

        /// <summary>
        /// The maximum number of API calls.
        /// </summary>
        public long MaxApiCalls { get; set; }

        /// <summary>
        /// The maximum allowed asset size.
        /// </summary>
        public long MaxAssetSize { get; set; }

        /// <summary>
        /// The maximum number of contributors.
        /// </summary>
        public int MaxContributors { get; set; }
    }
}
