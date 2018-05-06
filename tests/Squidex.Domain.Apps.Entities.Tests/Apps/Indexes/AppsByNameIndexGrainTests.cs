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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexGrainTests
    {
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IPersistence<AppsByNameIndexGrain.State> persistence = A.Fake<IPersistence<AppsByNameIndexGrain.State>>();
        private readonly Guid appId1 = Guid.NewGuid();
        private readonly Guid appId2 = Guid.NewGuid();
        private readonly string appName1 = "my-app1";
        private readonly string appName2 = "my-app2";
        private readonly AppsByNameIndexGrain sut;

        public AppsByNameIndexGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<string>.Ignored, A<Func<AppsByNameIndexGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new AppsByNameIndexGrain(store);
            sut.OnActivateAsync(SingleGrain.Id).Wait();
        }

        [Fact]
        public async Task Should_add_app_id_to_index()
        {
            await sut.AddAppAsync(appId1, appName1);

            var result = await sut.GetAppIdAsync(appName1);

            Assert.Equal(appId1, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_app_id_from_index()
        {
            await sut.AddAppAsync(appId1, appName1);
            await sut.RemoveAppAsync(appId1);

            var result = await sut.GetAppIdAsync(appName1);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.State>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_replace_app_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [appName1] = appId1,
                [appName2] = appId2
            };

            await sut.RebuildAsync(state);

            Assert.Equal(appId1, await sut.GetAppIdAsync(appName1));
            Assert.Equal(appId2, await sut.GetAppIdAsync(appName2));

            Assert.Equal(new List<Guid> { appId1, appId2 }, await sut.GetAppIdsAsync());

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }
    }
}
