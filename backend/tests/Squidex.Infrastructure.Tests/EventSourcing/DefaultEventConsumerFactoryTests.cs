// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventConsumerFactoryTests
    {
        private readonly IEventConsumer consumer1 = A.Fake<IEventConsumer>();
        private readonly IEventConsumer consumer2 = A.Fake<IEventConsumer>();
        private readonly DefaultEventConsumerFactory sut;

        public DefaultEventConsumerFactoryTests()
        {
            A.CallTo(() => consumer1.Name)
                .Returns("consumer1");

            A.CallTo(() => consumer2.Name)
                .Returns("consumer2");

            sut = new DefaultEventConsumerFactory(new[] { consumer1, consumer2 });
        }

        [Fact]
        public void Should_return_consumer_by_name()
        {
            var returned1 = sut.Create(consumer1.Name);
            var returned2 = sut.Create(consumer2.Name);

            Assert.Same(consumer1, returned1);
            Assert.Same(returned2, returned2);
        }

        [Fact]
        public void Should_throw_exception_for_invalid_name()
        {
            Assert.Throws<ArgumentException>(() => sut.Create("invalid"));
        }
    }
}
