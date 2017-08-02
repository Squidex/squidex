// ==========================================================================
//  IAppPlanBillingManager.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public interface IAppPlanBillingManager
    {
        bool HasPortal { get; }

        Task<IChangePlanResult> ChangePlanAsync(string userId, Guid appId, string appName, string planId);

        Task<string> GetPortalLinkAsync(string userId);
    }
}
