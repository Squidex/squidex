// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppUISettingsGrainTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<AppUISettingsGrain.State> persistence = A.Fake<IPersistence<AppUISettingsGrain.State>>();
        private readonly AppUISettingsGrain sut;

        public AppUISettingsGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<Guid>.Ignored, A<Func<AppUISettingsGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new AppUISettingsGrain(store);
            sut.OnActivateAsync(Guid.Empty).Wait();
        }

        [Fact]
        public async Task Should_set_setting()
        {
            await sut.SetAsync(new JObject(new JProperty("key", 15)).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                new JObject(
                    new JProperty("key", 15));

            Assert.Equal(expected.ToString(), actual.Value.ToString());
        }

        [Fact]
        public async Task Should_set_root_value()
        {
            await sut.SetAsync("key", ((JToken)123).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                new JObject(
                    new JProperty("key", 123));

            Assert.Equal(expected.ToString(), actual.Value.ToString());
        }

        [Fact]
        public async Task Should_remove_root_value()
        {
            await sut.SetAsync("key", ((JToken)123).AsJ());
            await sut.RemoveAsync("key");

            var actual = await sut.GetAsync();

            var expected = new JObject();

            Assert.Equal(expected.ToString(), actual.Value.ToString());
        }

        [Fact]
        public async Task Should_set_nested_value()
        {
            await sut.SetAsync("root.nested", ((JToken)123).AsJ());

            var actual = await sut.GetAsync();

            var expected =
                new JObject(
                    new JProperty("root",
                        new JObject(
                            new JProperty("nested", 123))));

            Assert.Equal(expected.ToString(), actual.Value.ToString());
        }

        [Fact]
        public async Task Should_remove_nested_value()
        {
            await sut.SetAsync("root.nested", ((JToken)123).AsJ());
            await sut.RemoveAsync("root.nested");

            var actual = await sut.GetAsync();

            var expected =
                new JObject(
                    new JProperty("root", new JObject()));

            Assert.Equal(expected.ToString(), actual.Value.ToString());
        }

        [Fact]
        public async Task Should_throw_exception_if_nested_not_an_object()
        {
            await sut.SetAsync("root.nested", ((JToken)123).AsJ());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SetAsync("root.nested.value", ((JToken)123).AsJ()));
        }

        [Fact]
        public Task Should_do_nothing_if_deleting_and_nested_not_found()
        {
            return sut.RemoveAsync("root.nested");
        }

        [Fact]
        public Task Should_do_nothing_if_deleting_and_key_not_found()
        {
            return sut.RemoveAsync("root");
        }
    }
}
