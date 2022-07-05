// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public class CounterGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IGrainState<CounterGrain.State> state = A.Fake<IGrainState<CounterGrain.State>>();
        private readonly CounterGrain sut;

        public CounterGrainTests()
        {
            sut = new CounterGrain(identity, state);
        }

        [Fact]
        public async Task Should_increment_counters()
        {
            Assert.Equal(1, await sut.IncrementAsync("Counter1"));
            Assert.Equal(2, await sut.IncrementAsync("Counter1"));

            Assert.Equal(1, await sut.IncrementAsync("Counter2"));
            Assert.Equal(2, await sut.IncrementAsync("Counter2"));

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappened(4, Times.Exactly);
        }

        [Fact]
        public async Task Should_reset_counter()
        {
            Assert.Equal(1, await sut.IncrementAsync("Counter1"));
            Assert.Equal(2, await sut.IncrementAsync("Counter1"));

            Assert.Equal(1, await sut.ResetAsync("Counter1", 1));

            Assert.Equal(2, await sut.IncrementAsync("Counter1"));

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappened(4, Times.Exactly);
        }
    }
}
