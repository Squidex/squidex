// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerManagerGrainTests
    {
        public class MyEventConsumerManagerGrain : EventConsumerManagerGrain
        {
            public MyEventConsumerManagerGrain(
                IEnumerable<IEventConsumer> eventConsumers,
                IGrainIdentity identity,
                IGrainRuntime runtime)
                : base(eventConsumers, identity, runtime)
            {
            }
        }

        private readonly IEventConsumer consumerA = A.Fake<IEventConsumer>();
        private readonly IEventConsumer consumerB = A.Fake<IEventConsumer>();
        private readonly IEventConsumerGrain grainA = A.Fake<IEventConsumerGrain>();
        private readonly IEventConsumerGrain grainB = A.Fake<IEventConsumerGrain>();
        private readonly MyEventConsumerManagerGrain sut;

        public EventConsumerManagerGrainTests()
        {
            var grainRuntime = A.Fake<IGrainRuntime>();
            var grainFactory = A.Fake<IGrainFactory>();

            A.CallTo(() => grainFactory.GetGrain<IEventConsumerGrain>("a", null)).Returns(grainA);
            A.CallTo(() => grainFactory.GetGrain<IEventConsumerGrain>("b", null)).Returns(grainB);
            A.CallTo(() => grainRuntime.GrainFactory).Returns(grainFactory);

            A.CallTo(() => consumerA.Name).Returns("a");
            A.CallTo(() => consumerA.EventsFilter).Returns("^a-");

            A.CallTo(() => consumerB.Name).Returns("b");
            A.CallTo(() => consumerB.EventsFilter).Returns("^b-");

            sut = new MyEventConsumerManagerGrain(new[] { consumerA, consumerB }, A.Fake<IGrainIdentity>(), grainRuntime);
        }

        [Fact]
        public async Task Should_not_activate_all_grains_on_activate()
        {
            await sut.OnActivateAsync();

            A.CallTo(() => grainA.ActivateAsync())
                .MustNotHaveHappened();

            A.CallTo(() => grainB.ActivateAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_activate_all_grains_on_reminder()
        {
            await sut.ReceiveReminder(null!, default);

            A.CallTo(() => grainA.ActivateAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.ActivateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_activate_all_grains_on_wakeup_with_null()
        {
            await sut.ActivateAsync(null);

            A.CallTo(() => grainA.ActivateAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.ActivateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_activate_matching_grains_if_stream_name_defined()
        {
            await sut.ActivateAsync("a-123");

            A.CallTo(() => grainA.ActivateAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.ActivateAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_start_all_grains()
        {
            await sut.StartAllAsync();

            A.CallTo(() => grainA.StartAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.StartAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_start_matching_grain()
        {
            await sut.StartAsync("a");

            A.CallTo(() => grainA.StartAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.StartAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_stop_all_grains()
        {
            await sut.StopAllAsync();

            A.CallTo(() => grainA.StopAsync())
                .MustHaveHappened();

            A.CallTo(() => grainB.StopAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_stop_matching_grain()
        {
            await sut.StopAsync("b");

            A.CallTo(() => grainA.StopAsync())
                .MustNotHaveHappened();

            A.CallTo(() => grainB.StopAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_reset_matching_grain()
        {
            await sut.ResetAsync("b");

            A.CallTo(() => grainA.ResetAsync())
                .MustNotHaveHappened();

            A.CallTo(() => grainB.ResetAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_fetch_infos_from_all_grains()
        {
            A.CallTo(() => grainA.GetStateAsync())
                .Returns(new EventConsumerInfo { Name = "A", Error = "A-Error", IsStopped = false, Position = "123" });

            A.CallTo(() => grainB.GetStateAsync())
                .Returns(new EventConsumerInfo { Name = "B", Error = "B-Error", IsStopped = false, Position = "456" });

            var infos = await sut.GetConsumersAsync();

            infos.Value.Should().BeEquivalentTo(
                new List<EventConsumerInfo>
                {
                    new EventConsumerInfo { Name = "A", Error = "A-Error", IsStopped = false, Position = "123" },
                    new EventConsumerInfo { Name = "B", Error = "B-Error", IsStopped = false, Position = "456" }
                });
        }
    }
}