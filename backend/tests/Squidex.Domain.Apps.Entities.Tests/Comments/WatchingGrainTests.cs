// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using NodaTime;
using Orleans.Core;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class WatchingGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IClock clock = A.Fake<IClock>();
        private readonly WatchingGrain sut;
        private Instant now = SystemClock.Instance.GetCurrentInstant();

        public WatchingGrainTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .ReturnsLazily(() => now);

            sut = new WatchingGrain(identity, clock);
        }

        [Fact]
        public async Task Should_only_return_self_if_no_one_watching()
        {
            var watching = await sut.GetWatchingUsersAsync("resource1", "user1");

            Assert.Equal(new[] { "user1" }, watching);
        }

        [Fact]
        public async Task Should_return_users_watching_on_same_resource()
        {
            await sut.GetWatchingUsersAsync("resource1", "user1");
            await sut.GetWatchingUsersAsync("resource2", "user2");

            var watching1 = await sut.GetWatchingUsersAsync("resource1", "user3");
            var watching2 = await sut.GetWatchingUsersAsync("resource2", "user4");

            Assert.Equal(new[] { "user1", "user3" }, watching1);
            Assert.Equal(new[] { "user2", "user4" }, watching2);
        }

        [Fact]
        public async Task Should_cleanup_old_users()
        {
            await sut.GetWatchingUsersAsync("resource1", "user1");
            await sut.GetWatchingUsersAsync("resource2", "user2");

            now = now.Plus(Duration.FromMinutes(2));

            sut.Cleanup();

            await sut.GetWatchingUsersAsync("resource1", "user3");
            await sut.GetWatchingUsersAsync("resource2", "user4");

            sut.Cleanup();

            var watching1 = await sut.GetWatchingUsersAsync("resource1", "user5");
            var watching2 = await sut.GetWatchingUsersAsync("resource2", "user6");

            Assert.Equal(new[] { "user3", "user5" }, watching1);
            Assert.Equal(new[] { "user4", "user6" }, watching2);
        }
    }
}
