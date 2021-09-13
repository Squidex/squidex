// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppUISettingsTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IAppUISettingsGrain grain = A.Fake<IAppUISettingsGrain>();
        private readonly AppUISettings sut;

        public AppUISettingsTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppUISettingsGrain>(A<string>._, null))
                .Returns(grain);

            sut = new AppUISettings(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_if_retrieving_settings()
        {
            var settings = JsonValue.Object();

            A.CallTo(() => grain.GetAsync())
                .Returns(settings.AsJ());

            var result = await sut.GetAsync(DomainId.NewGuid(), "user");

            Assert.Same(settings, result);
        }

        [Fact]
        public async Task Should_call_grain_if_setting_value()
        {
            var value = JsonValue.Object();

            await sut.SetAsync(DomainId.NewGuid(), "user", "the.path", value);

            A.CallTo(() => grain.SetAsync("the.path", A<J<IJsonValue>>.That.Matches(x => x.Value == value)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_replacing_settings()
        {
            var value = JsonValue.Object();

            await sut.SetAsync(DomainId.NewGuid(), "user", value);

            A.CallTo(() => grain.SetAsync(A<J<JsonObject>>.That.Matches(x => x.Value == value)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_removing_value()
        {
            await sut.RemoveAsync(DomainId.NewGuid(), "user", "the.path");

            A.CallTo(() => grain.RemoveAsync("the.path"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_clear_grain_when_app_deleted()
        {
            var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

            await ((IDeleter)sut).DeleteAppAsync(app, default);

            A.CallTo(() => grain.ClearAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_clear_grain_when_contributor_removed()
        {
            var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

            await ((IDeleter)sut).DeleteContributorAsync(app.Id, "user1", default);

            A.CallTo(() => grain.ClearAsync())
                .MustHaveHappened();
        }
    }
}
