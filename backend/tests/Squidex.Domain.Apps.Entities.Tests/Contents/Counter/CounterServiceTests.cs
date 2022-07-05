// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public class CounterServiceTests
    {
        private readonly TestState<CounterService.State> state;
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly CounterService sut;

        public CounterServiceTests()
        {
            state = new TestState<CounterService.State>(appId);

            sut = new CounterService(state.PersistenceFactory);
        }

        [Fact]
        public void Should_run_delete_with_default_order()
        {
            var order = ((IDeleter)sut).Order;

            Assert.Equal(0, order);
        }

        [Fact]
        public async Task Should_delete_state_when_app_deleted()
        {
            await ((IDeleter)sut).DeleteAppAsync(Mocks.App(NamedId.Of(appId, "my-app")), default);

            A.CallTo(() => state.Persistence.DeleteAsync(default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_increment_counters()
        {
            Assert.Equal(1, await sut.IncrementAsync(appId, "Counter1"));
            Assert.Equal(2, await sut.IncrementAsync(appId, "Counter1"));

            Assert.Equal(1, await sut.IncrementAsync(appId, "Counter2"));
            Assert.Equal(2, await sut.IncrementAsync(appId, "Counter2"));

            A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<CounterService.State>._, default))
                .MustHaveHappened(4, Times.Exactly);
        }

        [Fact]
        public async Task Should_reset_counter()
        {
            Assert.Equal(1, await sut.IncrementAsync(appId, "Counter1"));
            Assert.Equal(2, await sut.IncrementAsync(appId, "Counter1"));

            Assert.Equal(1, await sut.ResetAsync(appId, "Counter1", 1));

            Assert.Equal(2, await sut.IncrementAsync(appId, "Counter1"));

            A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<CounterService.State>._, default))
                .MustHaveHappened(4, Times.Exactly);
        }
    }
}
