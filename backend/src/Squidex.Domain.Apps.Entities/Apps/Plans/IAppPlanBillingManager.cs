// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public interface IAppPlanBillingManager
    {
        bool HasPortal { get; }

        Task<IChangePlanResult> ChangePlanAsync(string userId, NamedId<DomainId> appId, string? planId, string? referer);

        Task<string> GetPortalLinkAsync(string userId);
    }
}
