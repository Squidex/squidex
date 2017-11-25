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
        private readonly TaskFactory taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));

        public Task AddAppAsync(string appName)
        {
            return taskFactory.StartNew(() =>
            {
                State.AppNames.Add(appName);

                return WriteStateAsync();
            }).Unwrap();
        }

        public Task RemoveAppAsync(string appName)
        {
            return taskFactory.StartNew(() =>
            {
                State.AppNames.Remove(appName);

                return WriteStateAsync();
            }).Unwrap();
        }

        public Task<List<string>> GetAppNamesAsync()
        {
            return taskFactory.StartNew(() =>
            {
                return State.AppNames.ToList();
            });
        }
    }
}
