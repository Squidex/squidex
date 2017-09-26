// ==========================================================================
//  NoopAppPlanBillingManager.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Apps.Services.Implementations
{
    public sealed class NoopAppPlanBillingManager : IAppPlanBillingManager
    {
        public bool HasPortal
        {
            get { return false; }
        }

        public Task<IChangePlanResult> ChangePlanAsync(string userId, Guid appId, string appName, string planId)
        {
            return Task.FromResult<IChangePlanResult>(PlanChangedResult.Instance);
        }

        public Task<string> GetPortalLinkAsync(string userId)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
