// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexGrain : GrainOfString, IAppsByUserIndexGrain
    {
        private readonly IGrainState<GrainState> state;

        [CollectionName("Index_AppsByUser")]
        public sealed class GrainState
        {
            public HashSet<Guid> Apps { get; set; } = new HashSet<Guid>();
        }

        public AppsByUserIndexGrain(IGrainState<GrainState> state)
        {
            Guard.NotNull(state, nameof(state));

            this.state = state;
        }

        public Task RebuildAsync(HashSet<Guid> apps)
        {
            state.Value = new GrainState { Apps = apps };

            return state.WriteAsync();
        }

        public Task AddAppAsync(Guid appId)
        {
            state.Value.Apps.Add(appId);

            return state.WriteAsync();
        }

        public Task RemoveAppAsync(Guid appId)
        {
            state.Value.Apps.Remove(appId);

            return state.WriteAsync();
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(state.Value.Apps.ToList());
        }
    }
}
