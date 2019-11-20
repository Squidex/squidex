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
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetLoaderTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IAssetItemGrain grain = A.Fake<IAssetItemGrain>();
        private readonly Guid id = Guid.NewGuid();
        private readonly AssetLoader sut;

        public AssetLoaderTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAssetItemGrain>(id, null))
                .Returns(grain);

            sut = new AssetLoader(grainFactory);
        }

        [Fact]
        public async Task Should_throw_exception_if_no_state_returned()
        {
            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IAssetItemEntity>(null!));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetAsync(id, 10));
        }

        [Fact]
        public async Task Should_throw_exception_if_state_has_other_version()
        {
            var content = new AssetItemEntity { Version = 5 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IAssetItemEntity>(content));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetAsync(id, 10));
        }

        [Fact]
        public async Task Should_return_content_from_state()
        {
            var content = new AssetItemEntity { Version = 10 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IAssetItemEntity>(content));

            var result = await sut.GetAsync(id, 10);

            Assert.Same(content, result);
        }
    }
}
