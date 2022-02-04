// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class GrainWatchingService : IWatchingService
    {
        private readonly IGrainFactory grainFactory;

        public GrainWatchingService(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public Task<string[]> GetWatchingUsersAsync(DomainId appId, string resource, string userId)
        {
            Guard.NotNullOrEmpty(resource);
            Guard.NotNullOrEmpty(userId);

            return GetGrain(appId).GetWatchingUsersAsync(resource, userId);
        }

        private IWatchingGrain GetGrain(DomainId appId)
        {
            return grainFactory.GetGrain<IWatchingGrain>(appId.ToString());
        }
    }
}
