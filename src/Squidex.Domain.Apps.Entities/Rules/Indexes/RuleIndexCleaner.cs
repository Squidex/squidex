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

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RuleIndexCleaner : IAppStorage
    {
        private readonly IGrainFactory grainFactory;

        public RuleIndexCleaner(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task ClearAsync(Guid appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndex>(appId).ClearAsync();
        }
    }
}
