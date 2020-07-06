// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public interface IUsageTrackerGrain : IBackgroundGrain
    {
        Task AddTargetAsync(DomainId ruleId, NamedId<DomainId> appId, int limits, int? numDays);

        Task RemoveTargetAsync(DomainId ruleId);

        Task UpdateTargetAsync(DomainId ruleId, int limits, int? numDays);
    }
}