// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPlan
    {
        public RefToken Owner { get; }

        public string PlanId { get; }

        public AppPlan(RefToken owner, string planId)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNullOrEmpty(planId, nameof(planId));

            Owner = owner;

            PlanId = planId;
        }
    }
}
