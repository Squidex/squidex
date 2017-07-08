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

        Task ChangePlanAsync(string userId, Guid appId, string appName, string planId);

        Task<bool> HasPaymentOptionsAsync(string userId);

        Task<string> GetPortalLinkAsync(string userId);
    }
}
