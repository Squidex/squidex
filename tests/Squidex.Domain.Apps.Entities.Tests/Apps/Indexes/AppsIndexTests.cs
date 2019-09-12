// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IAppsByNameIndex index = A.Fake<IAppsByNameIndex>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly AppsIndex sut;

        public AppsIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id, null))
                .Returns(index);

            sut = new AppsIndex(grainFactory);
        }

        [Fact]
        public async Task Should_resolve_app_by_name()
        {
            var expected = SetupApp(0, false);

            var actual = await sut.GetAppAsync(appId.Name);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Should_resolve_app_by_id()
        {
            var expected = SetupApp(0, false);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Should_return_null_if_app_archived()
        {
            SetupApp(0, true);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Should_return_null_if_app_not_created()
        {
            SetupApp(EtagVersion.Empty, false);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Should_clean_index_if_not_consistent()
        {
            SetupApp(EtagVersion.Empty, false);

            await sut.GetAppAsync(appId.Name);

            A.CallTo(() => index.RemoveAppAsync(appId.Id)).MustHaveHappened();
        }

        private IAppEntity SetupApp(long version, bool archived)
        {
            var appEntity = A.Fake<IAppEntity>();

            A.CallTo(() => appEntity.Name)
                .Returns(appId.Name);
            A.CallTo(() => appEntity.Version)
                .Returns(version);
            A.CallTo(() => appEntity.IsArchived)
                .Returns(archived);

            var appGrain = A.Fake<IAppGrain>();

            A.CallTo(() => appGrain.GetStateAsync())
                .Returns(J.Of(appEntity));

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id, null))
                .Returns(appGrain);

            A.CallTo(() => index.GetAppIdAsync(appId.Name))
                .Returns(appId.Id);

            return appEntity;
        }
    }
}
