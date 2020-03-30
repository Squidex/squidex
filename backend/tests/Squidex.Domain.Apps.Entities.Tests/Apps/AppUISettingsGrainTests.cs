// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettingsGrainTests
    {
        private readonly IGrainState<AppUISettingsGrain.State> grainState = A.Fake<IGrainState<AppUISettingsGrain.State>>();
        private readonly AppUISettingsGrain sut;

        public AppUISettingsGrainTests()
        {
            sut = new AppUISettingsGrain(grainState);
        }

        [Fact]
        public async Task Should_set_setting()
        {
            await sut.SetAsync(JsonValue.Object().Add("key", 15).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                JsonValue.Object().Add("key", 15);

            Assert.Equal(expected.ToString(), actual.Value.ToString());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_set_root_value()
        {
            await sut.SetAsync("key", JsonValue.Create(123).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                JsonValue.Object().Add("key", 123);

            Assert.Equal(expected.ToString(), actual.Value.ToString());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_root_value()
        {
            await sut.SetAsync("key", JsonValue.Create(123).AsJ());

            await sut.RemoveAsync("key");

            var actual = await sut.GetAsync();

            var expected = JsonValue.Object();

            Assert.Equal(expected.ToString(), actual.Value.ToString());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_set_nested_value()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                JsonValue.Object().Add("root",
                    JsonValue.Object().Add("nested", 123));

            Assert.Equal(expected.ToString(), actual.Value.ToString());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_nested_value()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123).AsJ());

            await sut.RemoveAsync("root.nested");

            var actual = await sut.GetAsync();

            var expected =
                JsonValue.Object().Add("root",
                    JsonValue.Object());

            Assert.Equal(expected.ToString(), actual.Value.ToString());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_throw_exception_if_nested_not_an_object()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123).AsJ());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SetAsync("root.nested.value", JsonValue.Create(123).AsJ()));
        }

        [Fact]
        public async Task Should_do_nothing_if_deleting_and_nested_not_found()
        {
            await sut.RemoveAsync("root.nested");

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_deleting_and_key_not_found()
        {
            await sut.RemoveAsync("root");

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }
    }
}
