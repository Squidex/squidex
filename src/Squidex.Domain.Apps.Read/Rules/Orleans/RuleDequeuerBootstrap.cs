// ==========================================================================
//  RuleDequeuerBootstrap.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans.Providers;
using Squidex.Domain.Apps.Read.Rules.Orleans.Grains;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Rules.Orleans
{
    public sealed class RuleDequeuerBootstrap : IBootstrapProvider
    {
        public string Name { get; private set; }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var grain = providerRuntime.GrainFactory.GetGrain<IRuleDequeuerGrain>("Default");

            return grain.ActivateAsync();
        }

        public Task Close()
        {
            return TaskHelper.Done;
        }
    }
}
