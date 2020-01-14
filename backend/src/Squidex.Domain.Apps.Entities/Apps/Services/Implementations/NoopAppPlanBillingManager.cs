// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Services.Implementations
{
    public sealed class NoopAppPlanBillingManager : IAppPlanBillingManager
    {
        public bool HasPortal
        {
            get { return false; }
        }

        public Task<IChangePlanResult> ChangePlanAsync(string userId, NamedId<Guid> appId, string? planId)
        {
            return Task.FromResult<IChangePlanResult>(new PlanChangedResult());
        }

        public Task<string> GetPortalLinkAsync(string userId)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
