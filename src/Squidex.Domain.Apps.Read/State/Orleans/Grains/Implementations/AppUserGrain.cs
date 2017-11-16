// ==========================================================================
//  AppUserGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class AppUserGrain : GrainV2<AppUserGrainState>, IAppUserGrain
    {
        public AppUserGrain(IGrainRuntime runtime)
            : base(runtime)
        {
        }

        public Task AddAppAsync(string appName)
        {
            State.AppNames.Add(appName);

            return WriteStateAsync();
        }

        public Task RemoveAppAsync(string appName)
        {
            State.AppNames.Remove(appName);

            return WriteStateAsync();
        }

        public Task<List<string>> GetSchemaNamesAsync()
        {
            return Task.FromResult(State.AppNames.ToList());
        }
    }
}
