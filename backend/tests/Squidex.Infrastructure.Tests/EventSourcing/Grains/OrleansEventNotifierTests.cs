// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class OrleansEventNotifierTests
    {
        private readonly IEventConsumerManagerGrain manager = A.Fake<IEventConsumerManagerGrain>();
        private readonly OrleansEventNotifier sut;

        public OrleansEventNotifierTests()
        {
            var factory = A.Fake<IGrainFactory>();

            A.CallTo(() => factory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id, null))
                .Returns(manager);

            sut = new OrleansEventNotifier(factory);
        }

        [Fact]
        public void Should_wakeup_manager_with_stream_name()
        {
            sut.NotifyEventsStored("my-stream");

            A.CallTo(() => manager.ActivateAsync("my-stream"))
                .MustHaveHappened();
        }
    }
}
