// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class GrainWatchingServiceTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly GrainWatchingService sut;

        public GrainWatchingServiceTests()
        {
            sut = new GrainWatchingService(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_if_retrieving_watching_users()
        {
            var appId = DomainId.NewGuid();
            var userResource = "resource1";
            var userIdentity = "user1";

            var grain = A.Fake<IWatchingGrain>();

            A.CallTo(() => grainFactory.GetGrain<IWatchingGrain>(appId.ToString(), null))
                .Returns(grain);

            A.CallTo(() => grain.GetWatchingUsersAsync(userResource, userIdentity))
                .Returns(Task.FromResult(new[] { "user1", "user2" }));

            var result = await sut.GetWatchingUsersAsync(appId, userResource, userIdentity);

            Assert.Equal(new[] { "user1", "user2" }, result);
        }
    }
}
