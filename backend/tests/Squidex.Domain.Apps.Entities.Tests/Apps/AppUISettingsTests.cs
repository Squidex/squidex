﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
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
            A.CallTo(() => grainFactory.GetGrain<IAppUISettingsGrain>(A<string>.Ignored, null))
                .Returns(grain);

            sut = new AppUISettings(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_when_retrieving_settings()
        {
            var settings = JsonValue.Object();

            A.CallTo(() => grain.GetAsync())
                .Returns(settings.AsJ());

            var result = await sut.GetAsync(Guid.NewGuid(), "user");

            Assert.Same(settings, result);
        }

        [Fact]
        public async Task Should_call_grain_when_setting_value()
        {
            var value = JsonValue.Object();

            await sut.SetAsync(Guid.NewGuid(), "user", "the.path", value);

            A.CallTo(() => grain.SetAsync("the.path", A<J<IJsonValue>>.That.Matches(x => x.Value == value)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_replacing_settings()
        {
            var value = JsonValue.Object();

            await sut.SetAsync(Guid.NewGuid(), "user", value);

            A.CallTo(() => grain.SetAsync(A<J<JsonObject>>.That.Matches(x => x.Value == value)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_removing_value()
        {
            await sut.RemoveAsync(Guid.NewGuid(), "user", "the.path");

            A.CallTo(() => grain.RemoveAsync("the.path"))
                .MustHaveHappened();
        }
    }
}
