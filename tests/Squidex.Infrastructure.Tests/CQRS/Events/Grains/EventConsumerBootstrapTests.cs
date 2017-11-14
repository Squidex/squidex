// ==========================================================================
//  EventConsumerBootstrapTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Orleans.Providers;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events.Grains
{
    public sealed class EventConsumerBootstrapTests
    {
        private readonly IEventConsumerRegistryGrain registry = A.Fake<IEventConsumerRegistryGrain>();
        private readonly IProviderRuntime runtime = A.Fake<IProviderRuntime>();
        private readonly EventConsumerBootstrap sut = new EventConsumerBootstrap();

        public EventConsumerBootstrapTests()
        {
            var factory = A.Fake<IGrainFactory>();

            A.CallTo(() => factory.GetGrain<IEventConsumerRegistryGrain>("Default", null))
                .Returns(registry);

            A.CallTo(() => runtime.GrainFactory)
                .Returns(factory);
        }

        [Fact]
        public async Task Should_do_nothing_on_close()
        {
            await sut.Close();
        }

        [Fact]
        public async Task Should_set_name_on_init()
        {
            await sut.Init("MyName", runtime, null);

            Assert.Equal("MyName", sut.Name);
        }

        [Fact]
        public async Task Should_activate_registry_on_init()
        {
            await sut.Init("MyName", runtime, null);

            A.CallTo(() => registry.ActivateAsync(null))
                .MustHaveHappened();
        }
    }
}
