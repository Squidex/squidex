// ==========================================================================
//  ChangePlan.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class ChangePlan : AppAggregateCommand
    {
        public bool FromCallback { get; set; }

        public string PlanId { get; set; }
    }
}
