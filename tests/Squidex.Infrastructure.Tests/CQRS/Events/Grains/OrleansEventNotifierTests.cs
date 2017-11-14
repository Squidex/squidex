// ==========================================================================
//  OrleansEventNotifierTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events.Grains
{
    public class OrleansEventNotifierTests
    {
        private readonly IEventConsumerRegistryGrain registry = A.Fake<IEventConsumerRegistryGrain>();
        private readonly OrleansEventNotifier sut;

        public OrleansEventNotifierTests()
        {
            var factory = A.Fake<IGrainFactory>();

            A.CallTo(() => factory.GetGrain<IEventConsumerRegistryGrain>("Default", null))
                .Returns(registry);

            sut = new OrleansEventNotifier(factory);
        }

        [Fact]
        public void Should_activate_registry_with_stream_name()
        {
            sut.NotifyEventsStored("my-stream");

            A.CallTo(() => registry.ActivateAsync("my-stream"))
                .MustHaveHappened();
        }
    }
}
