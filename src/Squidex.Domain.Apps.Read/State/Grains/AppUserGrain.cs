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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public sealed class AppUserGrain : StatefulObject<AppUserGrainState>
    {
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher();

        public Task AddAppAsync(string appName)
        {
            return dispatcher.DispatchAndUnwrapAsync(() =>
            {
                State.AppNames.Add(appName);

                return WriteStateAsync();
            });
        }

        public Task RemoveAppAsync(string appName)
        {
            return dispatcher.DispatchAndUnwrapAsync(() =>
            {
                State.AppNames.Remove(appName);

                return WriteStateAsync();
            });
        }

        public Task<List<string>> GetAppNamesAsync()
        {
            return dispatcher.DispatchAndUnwrapAsync(() =>
            {
                return Task.FromResult(State.AppNames.ToList());
            });
        }
    }
}
