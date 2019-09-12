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
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsByNameIndexGrainTests
    {
        private readonly IGrainState<AppsByNameIndexGrain.GrainState> grainState = A.Fake<IGrainState<AppsByNameIndexGrain.GrainState>>();
        private readonly NamedId<Guid> appId1 = NamedId.Of(Guid.NewGuid(), "my-app1");
        private readonly NamedId<Guid> appId2 = NamedId.Of(Guid.NewGuid(), "my-app2");
        private readonly AppsByNameIndexGrain sut;

        public AppsByNameIndexGrainTests()
        {
            sut = new AppsByNameIndexGrain(grainState);
            sut.ActivateAsync(SingleGrain.Id).Wait();
        }

        [Fact]
        public async Task Should_add_app_id_to_index()
        {
            Assert.True(await sut.AddAppAsync(appId1.Id, appId1.Name));

            var result = await sut.GetAppIdAsync(appId1.Name);

            Assert.Equal(appId1.Id, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_index_if_name_taken()
        {
            await sut.AddAppAsync(appId2.Id, appId1.Name);

            Assert.False(await sut.AddAppAsync(appId1.Id, appId1.Name, true));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_name_reserved()
        {
            await sut.AddAppAsync(appId2.Id, appId1.Name, true);

            Assert.False(await sut.AddAppAsync(appId1.Id, appId1.Name, true));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_id_taken()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name);

            Assert.False(await sut.AddAppAsync(appId1.Id, appId2.Name, true));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_id_reserved()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name, true);

            Assert.False(await sut.AddAppAsync(appId1.Id, appId2.Name, true));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_if_id_and_name_not_reserved()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name);

            Assert.True(await sut.AddAppAsync(appId2.Id, appId2.Name, true));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_after_app_removed()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name);

            await sut.RemoveAppAsync(appId1.Id);

            Assert.True(await sut.AddAppAsync(appId1.Id, appId1.Name, true));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_after_reservation_removed()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name, true);

            await sut.RemoveReservationAsync(appId1.Id, appId1.Name);

            Assert.True(await sut.AddAppAsync(appId1.Id, appId1.Name, true));
        }

        [Fact]
        public async Task Should_return_many_app_ids()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name);
            await sut.AddAppAsync(appId2.Id, appId2.Name);

            var ids = await sut.GetAppIdsAsync(appId1.Name, appId2.Name);

            Assert.Equal(new List<Guid> { appId1.Id, appId2.Id }, ids);
        }

        [Fact]
        public async Task Should_remove_app_id_from_index()
        {
            await sut.AddAppAsync(appId1.Id, appId1.Name);
            await sut.RemoveAppAsync(appId1.Id);

            var result = await sut.GetAppIdAsync(appId1.Name);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_replace_app_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [appId1.Name] = appId1.Id,
                [appId2.Name] = appId2.Id
            };

            await sut.RebuildAsync(state);

            Assert.Equal(appId1.Id, await sut.GetAppIdAsync(appId1.Name));
            Assert.Equal(appId2.Id, await sut.GetAppIdAsync(appId2.Name));

            Assert.Equal(new List<Guid> { appId1.Id, appId2.Id }, await sut.GetAppIdsAsync());

            Assert.Equal(2, await sut.CountAsync());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }
    }
}
