// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexGrainTests
    {
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IPersistence<AppsByUserIndexGrain.State> persistence = A.Fake<IPersistence<AppsByUserIndexGrain.State>>();
        private readonly Guid appId1 = Guid.NewGuid();
        private readonly Guid appId2 = Guid.NewGuid();
        private readonly AppsByUserIndexGrain sut;

        public AppsByUserIndexGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<string>.Ignored, A<Func<AppsByUserIndexGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new AppsByUserIndexGrain(store);
            sut.OnActivateAsync("user").Wait();
        }

        [Fact]
        public async Task Should_add_app_id_to_index()
        {
            await sut.AddAppAsync(appId1);
            await sut.AddAppAsync(appId2);

            var result = await sut.GetAppIdsAsync();

            Assert.Equal(new List<Guid> { appId1, appId2 }, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByUserIndexGrain.State>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_remove_app_id_from_index()
        {
            await sut.AddAppAsync(appId1);
            await sut.AddAppAsync(appId2);
            await sut.RemoveAppAsync(appId1);

            var result = await sut.GetAppIdsAsync();

            Assert.Equal(new List<Guid> { appId2 }, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByUserIndexGrain.State>.Ignored))
                .MustHaveHappenedTwiceOrMore();
        }

        [Fact]
        public async Task Should_replace_app_ids_on_rebuild()
        {
            var state = HashSet.Of(appId1, appId2);

            await sut.RebuildAsync(state);

            var result = await sut.GetAppIdsAsync();

            Assert.Equal(new List<Guid> { appId1, appId2 }, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByUserIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }
    }
}
