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
        Task AddTargetAsync(NamedId<Guid> appId, int limits);

        Task ActivateTargetAsync(NamedId<Guid> appId);

        Task DeactivateTargetAsync(NamedId<Guid> appId);

        Task RemoveTargetAsync(NamedId<Guid> appId);
    }
}