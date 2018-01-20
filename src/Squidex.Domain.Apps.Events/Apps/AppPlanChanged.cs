// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppPlanChanged")]
    public sealed class AppPlanChanged : AppEvent
    {
        public string PlanId { get; set; }
    }
}
