// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetLoaderTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IAssetGrain grain = A.Fake<IAssetGrain>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId id = DomainId.NewGuid();
        private readonly AssetLoader sut;

        public AssetLoaderTests()
        {
            var key = DomainId.Combine(appId, id).ToString();

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(key, null))
                .Returns(grain);

            sut = new AssetLoader(grainFactory);
        }

        [Fact]
        public async Task Should_return_null_if_no_state_returned()
        {
            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(Task.FromResult<IAssetEntity>(null!));

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_return_null_if_state_empty()
        {
            var asset = new AssetEntity { Version = EtagVersion.Empty };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(asset);

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_return_null_if_state_has_other_version()
        {
            var asset = new AssetEntity { Version = 5 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(asset);

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_return_content_from_state()
        {
            var asset = new AssetEntity { Version = 10 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(asset);

            var result = await sut.GetAsync(appId, id, 10);

            Assert.Same(asset, result);
        }
    }
}
