// ==========================================================================
//  EventConsumerBootstrap.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans.Providers;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains
{
    public sealed class EventConsumerBootstrap : IBootstrapProvider
    {
        public string Name { get; private set; }

        public Task Close()
        {
            return TaskHelper.Done;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            return providerRuntime.GrainFactory.GetGrain<IEventConsumerRegistryGrain>("Default").ActivateAsync(null);
        }
    }
}
