// ==========================================================================
//  PlanChangedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public sealed class PlanChangedResult : IChangePlanResult
    {
        public static readonly PlanChangedResult Instance = new PlanChangedResult();

        private PlanChangedResult()
        {
        }
    }
}
