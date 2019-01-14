// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public interface IUsageTrackerGrain : IGrainWithStringKey, IBackgroundGrain
    {
        Task AddTargetAsync(Guid ruleId, NamedId<Guid> appId, int limits, int? numDays);

        Task RemoveTargetAsync(Guid ruleId);

        Task UpdateTargetAsync(Guid ruleId, int limits, int? numDays);
    }
}