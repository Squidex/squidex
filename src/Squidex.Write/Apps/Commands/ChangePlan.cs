// ==========================================================================
//  ChangePlanCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Write.Apps.Commands
{
    public sealed class ChangePlan : AppAggregateCommand
    {
        public string PlanId { get; set; }
    }
}
