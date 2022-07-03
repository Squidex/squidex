// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettingsGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IGrainState<AppUISettingsGrain.State> state = A.Fake<IGrainState<AppUISettingsGrain.State>>();
        private readonly AppUISettingsGrain sut;

        public AppUISettingsGrainTests()
        {
            sut = new AppUISettingsGrain(identity, state);
        }

        [Fact]
        public async Task Should_set_setting()
        {
            await sut.SetAsync(new JsonObject().Add("key", 15));

            var actual = await sut.GetAsync();

            var expected =
                new JsonObject().Add("key", 15);

            Assert.Equal(expected.ToString(), actual.ToString());

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_set_root_value()
        {
            await sut.SetAsync("key", JsonValue.Create(123));

            var actual = await sut.GetAsync();

            var expected =
                new JsonObject().Add("key", 123);

            Assert.Equal(expected.ToString(), actual.ToString());

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_root_value()
        {
            await sut.SetAsync("key", JsonValue.Create(123));

            await sut.RemoveAsync("key");

            var actual = await sut.GetAsync();

            var expected = new JsonObject();

            Assert.Equal(expected.ToString(), actual.ToString());

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_set_nested_value()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123));

            var actual = await sut.GetAsync();

            var expected =
                new JsonObject().Add("root",
                    new JsonObject().Add("nested", 123));

            Assert.Equal(expected.ToString(), actual.ToString());

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_nested_value()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123));

            await sut.RemoveAsync("root.nested");

            var actual = await sut.GetAsync();

            var expected =
                new JsonObject().Add("root",
                    new JsonObject());

            Assert.Equal(expected.ToString(), actual.ToString());

            A.CallTo(() => state.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_throw_exception_if_nested_not_an_object()
        {
            await sut.SetAsync("root.nested", JsonValue.Create(123));

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SetAsync("root.nested.value", JsonValue.Create(123)));
        }

        [Fact]
        public async Task Should_do_nothing_if_deleting_and_nested_not_found()
        {
            await sut.RemoveAsync("root.nested");

            A.CallTo(() => state.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_deleting_and_key_not_found()
        {
            await sut.RemoveAsync("root");

            A.CallTo(() => state.WriteAsync())
                .MustNotHaveHappened();
        }
    }
}
