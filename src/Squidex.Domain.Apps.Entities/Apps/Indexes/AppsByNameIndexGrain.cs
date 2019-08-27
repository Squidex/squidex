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
    public sealed class AppsByNameIndexGrain : GrainOfString, IAppsByNameIndex
    {
        private readonly HashSet<Guid> reservedIds = new HashSet<Guid>();
        private readonly HashSet<string> reservedNames = new HashSet<string>();
        private readonly IGrainState<GrainState> state;

        [CollectionName("Index_AppsByName")]
        public sealed class GrainState
        {
            public Dictionary<string, Guid> Apps { get; set; } = new Dictionary<string, Guid>(StringComparer.Ordinal);
        }

        public AppsByNameIndexGrain(IGrainState<GrainState> state)
        {
            this.state = state;
        }

        public Task RebuildAsync(Dictionary<string, Guid> apps)
        {
            state.Value = new GrainState { Apps = apps };

            return state.WriteAsync();
        }

        public Task<bool> ReserveAppAsync(Guid appId, string name)
        {
            var canReserve = !IsInUse(appId, name) && !IsReserved(appId, name);

            if (canReserve)
            {
                reservedIds.Add(appId);
                reservedNames.Add(name);
            }

            return Task.FromResult(canReserve);
        }

        private bool IsInUse(Guid appId, string name)
        {
            return state.Value.Apps.ContainsKey(name) || state.Value.Apps.Any(x => x.Value == appId);
        }

        private bool IsReserved(Guid appId, string name)
        {
            return reservedIds.Contains(appId) || reservedNames.Contains(name);
        }

        public Task RemoveReservationAsync(Guid appId, string name)
        {
            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return TaskHelper.Done;
        }

        public Task AddAppAsync(Guid appId, string name)
        {
            state.Value.Apps[name] = appId;

            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return state.WriteAsync();
        }

        public Task RemoveAppAsync(Guid appId)
        {
            var name = state.Value.Apps.FirstOrDefault(x => x.Value == appId).Key;

            if (!string.IsNullOrWhiteSpace(name))
            {
                state.Value.Apps.Remove(name);

                reservedIds.Remove(appId);
                reservedNames.Remove(name);
            }

            return state.WriteAsync();
        }

        public Task<List<Guid>> GetAppIdsAsync(params string[] names)
        {
            var appIds = new List<Guid>();

            foreach (var appName in names)
            {
                if (state.Value.Apps.TryGetValue(appName, out var appId))
                {
                    appIds.Add(appId);
                }
            }

            return Task.FromResult(appIds);
        }

        public Task<Guid> GetAppIdAsync(string appName)
        {
            state.Value.Apps.TryGetValue(appName, out var appId);

            return Task.FromResult(appId);
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(state.Value.Apps.Values.ToList());
        }

        public Task<long> CountAsync()
        {
            return Task.FromResult((long)state.Value.Apps.Count);
        }
    }
}
