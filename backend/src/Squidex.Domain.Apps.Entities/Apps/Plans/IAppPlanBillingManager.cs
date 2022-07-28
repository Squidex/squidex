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

        Task<Uri?> MustRedirectToPortalAsync(string userId, NamedId<DomainId> appId, string? planId,
            CancellationToken ct = default);

        Task SubscribeAsync(string userId, NamedId<DomainId> appId, string planId,
            CancellationToken ct = default);

        Task UnsubscribeAsync(string userId, NamedId<DomainId> appId,
            CancellationToken ct = default);

        Task<string> GetPortalLinkAsync(string userId,
            CancellationToken ct = default);
    }
}
