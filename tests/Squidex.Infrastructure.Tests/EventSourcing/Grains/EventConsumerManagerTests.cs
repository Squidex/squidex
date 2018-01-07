// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.EventSourcing.Grains.Messages;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerManagerTests
    {
        private readonly EventConsumerGrain actor1 = A.Fake<EventConsumerGrain>();
        private readonly EventConsumerGrain actor2 = A.Fake<EventConsumerGrain>();
        private readonly IStateFactory factory = A.Fake<IStateFactory>();
        private readonly IEventConsumer consumer1 = A.Fake<IEventConsumer>();
        private readonly IEventConsumer consumer2 = A.Fake<IEventConsumer>();
        private readonly IPubSub pubSub = new InMemoryPubSub();
        private readonly string consumerName1 = "Consumer1";
        private readonly string consumerName2 = "Consumer2";
        private readonly EventConsumerGrainManager sut;

        public EventConsumerManagerTests()
        {
            A.CallTo(() => consumer1.Name).Returns(consumerName1);
            A.CallTo(() => consumer2.Name).Returns(consumerName2);

            A.CallTo(() => factory.CreateAsync<EventConsumerGrain>(consumerName1)).Returns(actor1);
            A.CallTo(() => factory.CreateAsync<EventConsumerGrain>(consumerName2)).Returns(actor2);

            sut = new EventConsumerGrainManager(new IEventConsumer[] { consumer1, consumer2 }, pubSub, factory);
        }

        [Fact]
        public void Should_activate_all_actors()
        {
            sut.Run();

            A.CallTo(() => actor1.Activate(consumer1))
                .MustHaveHappened();

            A.CallTo(() => actor2.Activate(consumer2))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_start_correct_actor()
        {
            sut.Run();

            pubSub.Publish(new StartConsumerMessage { ConsumerName = consumerName1 }, true);

            A.CallTo(() => actor1.Start())
                .MustHaveHappened();

            A.CallTo(() => actor2.Start())
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_stop_correct_actor()
        {
            sut.Run();

            pubSub.Publish(new StopConsumerMessage { ConsumerName = consumerName1 }, true);

            A.CallTo(() => actor1.Stop())
                .MustHaveHappened();

            A.CallTo(() => actor2.Stop())
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_reset_correct_actor()
        {
            sut.Run();

            pubSub.Publish(new ResetConsumerMessage { ConsumerName = consumerName2 }, true);

            A.CallTo(() => actor1.Reset())
                .MustNotHaveHappened();

            A.CallTo(() => actor2.Reset())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_get_state_from_all_actors()
        {
            sut.Run();

            A.CallTo(() => actor1.GetState())
                .Returns(new EventConsumerInfo { Name = consumerName1, Position = "123 " });

            A.CallTo(() => actor2.GetState())
                .Returns(new EventConsumerInfo { Name = consumerName2, Position = "345 " });

            var response = await pubSub.RequestAsync<GetStatesRequest, GetStatesResponse>(new GetStatesRequest(), TimeSpan.FromSeconds(5), true);

            response.States.ShouldAllBeEquivalentTo(new EventConsumerInfo[]
            {
                new EventConsumerInfo { Name = consumerName1, Position = "123 " },
                new EventConsumerInfo { Name = consumerName2, Position = "345 " }
            });
        }

        [Fact]
        public void Should_not_dispose_actors()
        {
            sut.Dispose();
        }
    }
}
