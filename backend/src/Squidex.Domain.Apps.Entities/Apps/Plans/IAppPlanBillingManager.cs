// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public interface IAppPlanBillingManager
    {
        bool HasPortal { get; }

        Task<IChangePlanResult> ChangePlanAsync(string userId, NamedId<DomainId> appId, string? planId, string? referer,
            CancellationToken ct = default);

        Task<string> GetPortalLinkAsync(string userId,
            CancellationToken ct = default);
    }
}
