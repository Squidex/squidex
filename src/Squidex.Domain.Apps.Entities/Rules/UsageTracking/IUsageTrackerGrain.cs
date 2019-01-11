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

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public interface IUsageTrackerGrain : IGrainWithStringKey
    {
        Task AddTargetAsync(Guid ruleId, NamedId<Guid> appId, int limits);

        Task ActivateTargetAsync(Guid ruleId);

        Task DeactivateTargetAsync(Guid ruleId);

        Task RemoveTargetAsync(Guid ruleId);

        Task UpdateTargetAsync(Guid ruleId, int limits);
    }
}