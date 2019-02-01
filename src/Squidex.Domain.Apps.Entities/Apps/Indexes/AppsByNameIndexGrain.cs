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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexGrain : GrainOfString<AppsByNameIndexGrain.GrainState>, IAppsByNameIndex
    {
        private readonly HashSet<Guid> reservedIds = new HashSet<Guid>();
        private readonly HashSet<string> reservedNames = new HashSet<string>();

        [CollectionName("Index_AppsByName")]
        public sealed class GrainState
        {
            public Dictionary<string, Guid> Apps { get; set; } = new Dictionary<string, Guid>(StringComparer.Ordinal);
        }

        public AppsByNameIndexGrain(IStore<string> store)
            : base(store)
        {
        }

        public Task RebuildAsync(Dictionary<string, Guid> apps)
        {
            State = new GrainState { Apps = apps };

            return WriteStateAsync();
        }

        public Task<bool> ReserveAppAsync(Guid appId, string name)
        {
            var canReserve =
                !State.Apps.ContainsKey(name) &&
                !State.Apps.Any(x => x.Value == appId) &&
                !reservedIds.Contains(appId) &&
                !reservedNames.Contains(name);

            if (canReserve)
            {
                reservedIds.Add(appId);
                reservedNames.Add(name);
            }

            return Task.FromResult(canReserve);
        }

        public Task RemoveReservationAsync(Guid appId, string name)
        {
            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return TaskHelper.Done;
        }

        public Task AddAppAsync(Guid appId, string name)
        {
            State.Apps[name] = appId;

            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return WriteStateAsync();
        }

        public Task RemoveAppAsync(Guid appId)
        {
            var name = State.Apps.FirstOrDefault(x => x.Value == appId).Key;

            if (!string.IsNullOrWhiteSpace(name))
            {
                State.Apps.Remove(name);

                reservedIds.Remove(appId);
                reservedNames.Remove(name);
            }

            return WriteStateAsync();
        }

        public Task<List<Guid>> GetAppIdsAsync(params string[] names)
        {
            var appIds = new List<Guid>();

            foreach (var appName in names)
            {
                if (State.Apps.TryGetValue(appName, out var appId))
                {
                    appIds.Add(appId);
                }
            }

            return Task.FromResult(appIds);
        }

        public Task<Guid> GetAppIdAsync(string appName)
        {
            State.Apps.TryGetValue(appName, out var appId);

            return Task.FromResult(appId);
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(State.Apps.Values.ToList());
        }

        public Task<long> CountAsync()
        {
            return Task.FromResult((long)State.Apps.Count);
        }
    }
}
