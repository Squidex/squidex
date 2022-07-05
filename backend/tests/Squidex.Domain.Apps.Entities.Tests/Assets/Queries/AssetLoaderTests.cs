// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetLoaderTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
        private readonly AssetDomainObject domainObject = A.Fake<AssetDomainObject>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId id = DomainId.NewGuid();
        private readonly AssetLoader sut;

        public AssetLoaderTests()
        {
            ct = cts.Token;

            var key = DomainId.Combine(appId, id);

            A.CallTo(() => domainObjectFactory.Create<AssetDomainObject>(key))
                .Returns(domainObject);

            sut = new AssetLoader(domainObjectFactory);
        }

        [Fact]
        public async Task Should_return_null_if_no_state_returned()
        {
            var asset = (AssetDomainObject.State)null!;

            A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
                .Returns(asset);

            Assert.Null(await sut.GetAsync(appId, id, 10, ct));
        }

        [Fact]
        public async Task Should_return_null_if_state_empty()
        {
            var asset = new AssetDomainObject.State { Version = EtagVersion.Empty };

            A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
                .Returns(asset);

            Assert.Null(await sut.GetAsync(appId, id, 10, ct));
        }

        [Fact]
        public async Task Should_return_null_if_state_has_other_version()
        {
            var asset = new AssetDomainObject.State { Version = 5 };

            A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
                .Returns(asset);

            Assert.Null(await sut.GetAsync(appId, id, 10, ct));
        }

        [Fact]
        public async Task Should_not_return_null_if_state_has_other_version_than_any()
        {
            var asset = new AssetDomainObject.State { Version = 5 };

            A.CallTo(() => domainObject.GetSnapshotAsync(EtagVersion.Any, ct))
                .Returns(asset);

            var result = await sut.GetAsync(appId, id, EtagVersion.Any, ct);

            Assert.Same(asset, result);
        }

        [Fact]
        public async Task Should_return_asset_from_state()
        {
            var asset = new AssetDomainObject.State { Version = 10 };

            A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
                .Returns(asset);

            var result = await sut.GetAsync(appId, id, 10, ct);

            Assert.Same(asset, result);
        }
    }
}
