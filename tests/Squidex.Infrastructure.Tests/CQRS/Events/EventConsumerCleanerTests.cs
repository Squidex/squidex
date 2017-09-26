// ==========================================================================
//  EventConsumerCleanerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EventConsumerCleanerTests
    {
        [Fact]
        public async Task Should_call_repository_with_all_names()
        {
            var eventConsumer1 = A.Fake<IEventConsumer>();
            var eventConsumer2 = A.Fake<IEventConsumer>();

            A.CallTo(() => eventConsumer1.Name).Returns("consumer1");
            A.CallTo(() => eventConsumer2.Name).Returns("consumer2");

            var repository = A.Fake<IEventConsumerInfoRepository>();

            var sut = new EventConsumerCleaner(new[] { eventConsumer1, eventConsumer2 }, repository);

            await sut.CleanAsync();

            A.CallTo(() => repository.ClearAsync(A<IEnumerable<string>>.That.IsSameSequenceAs(new string[] { "consumer1", "consumer2" }))).MustHaveHappened();
        }
    }
}
