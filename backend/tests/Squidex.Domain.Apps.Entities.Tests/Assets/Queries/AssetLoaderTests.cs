// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetLoaderTests : GivenContext
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly AssetDomainObject domainObject = A.Fake<AssetDomainObject>();
    private readonly DomainId id = DomainId.NewGuid();
    private readonly DomainId uniqueId;
    private readonly AssetLoader sut;

    public AssetLoaderTests()
    {
        uniqueId = DomainId.Combine(AppId.Id, id);

        A.CallTo(() => domainObjectCache.GetAsync<AssetDomainObject.State>(A<DomainId>._, A<long>._, CancellationToken))
            .Returns(Task.FromResult<AssetDomainObject.State>(null!));

        A.CallTo(() => domainObjectFactory.Create<AssetDomainObject>(uniqueId))
            .Returns(domainObject);

        sut = new AssetLoader(domainObjectFactory, domainObjectCache);
    }

    [Fact]
    public async Task Should_return_null_if_no_state_returned()
    {
        var asset = (AssetDomainObject.State)null!;

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(asset);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_return_null_if_state_empty()
    {
        var asset = new AssetDomainObject.State { Version = EtagVersion.Empty };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(asset);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_return_null_if_state_has_other_version()
    {
        var asset = new AssetDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(asset);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_not_return_null_if_state_has_other_version_than_any()
    {
        var asset = new AssetDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(EtagVersion.Any, CancellationToken))
            .Returns(asset);

        var actual = await sut.GetAsync(AppId.Id, id, EtagVersion.Any, CancellationToken);

        Assert.Same(asset, actual);
    }

    [Fact]
    public async Task Should_return_asset_from_state()
    {
        var asset = new AssetDomainObject.State { Version = 10 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(asset);

        var actual = await sut.GetAsync(AppId.Id, id, 10, CancellationToken);

        Assert.Same(asset, actual);
    }

    [Fact]
    public async Task Should_return_content_from_cache()
    {
        var content = new AssetDomainObject.State { Version = 10 };

        A.CallTo(() => domainObjectCache.GetAsync<AssetDomainObject.State>(DomainId.Combine(AppId.Id, id), 10, CancellationToken))
            .Returns(content);

        var actual = await sut.GetAsync(AppId.Id, id, 10, CancellationToken);

        Assert.Same(content, actual);

        A.CallTo(() => domainObjectFactory.Create<AssetDomainObject>(uniqueId))
            .MustNotHaveHappened();
    }
}
