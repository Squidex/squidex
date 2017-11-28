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
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed class AppUserGrain : StatefulObject<AppUserGrainState>
    {
        public Task AddAppAsync(string appName)
        {
            State = State.AddApp(appName);

            return WriteStateAsync();
        }

        public Task RemoveAppAsync(string appName)
        {
            State = State.RemoveApp(appName);

            return WriteStateAsync();
        }

        public Task<List<string>> GetAppNamesAsync()
        {
            return Task.FromResult(State.AppNames.ToList());
        }
    }
}
