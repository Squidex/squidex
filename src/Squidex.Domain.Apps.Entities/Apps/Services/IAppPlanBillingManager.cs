﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Services
{
    public interface IAppPlanBillingManager
    {
        bool HasPortal { get; }

        Task<IChangePlanResult> ChangePlanAsync(string userId, Guid appId, string appName, string planId);

        Task<string> GetPortalLinkAsync(string userId);
    }
}
