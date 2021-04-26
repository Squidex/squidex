// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class NoopAppPlanBillingManager : IAppPlanBillingManager
    {
        public bool HasPortal
        {
            get => false;
        }

        public Task<IChangePlanResult> ChangePlanAsync(string userId, NamedId<DomainId> appId, string? planId, string? referer)
        {
            return Task.FromResult<IChangePlanResult>(new PlanChangedResult());
        }

        public Task<string> GetPortalLinkAsync(string userId)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
