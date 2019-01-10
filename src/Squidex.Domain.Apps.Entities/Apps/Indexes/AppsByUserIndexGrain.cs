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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexGrain : GrainOfString<AppsByUserIndexGrain.GrainState>, IAppsByUserIndex
    {
        [CollectionName("Index_AppsByUser")]
        public sealed class GrainState
        {
            public HashSet<Guid> Apps { get; set; } = new HashSet<Guid>();
        }

        public AppsByUserIndexGrain(IStore<string> store)
            : base(store)
        {
        }

        public Task RebuildAsync(HashSet<Guid> apps)
        {
            State = new GrainState { Apps = apps };

            return WriteStateAsync();
        }

        public Task AddAppAsync(Guid appId)
        {
            State.Apps.Add(appId);

            return WriteStateAsync();
        }

        public Task RemoveAppAsync(Guid appId)
        {
            State.Apps.Remove(appId);

            return WriteStateAsync();
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(State.Apps.ToList());
        }
    }
}
