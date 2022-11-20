// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentLoaderTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly ContentDomainObject domainObject = A.Fake<ContentDomainObject>();
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly DomainId id = DomainId.NewGuid();
    private readonly DomainId unqiueId;
    private readonly ContentLoader sut;

    public ContentLoaderTests()
    {
        ct = cts.Token;

        unqiueId = DomainId.Combine(appId, id);

        A.CallTo(() => domainObjectCache.GetAsync<ContentDomainObject.State>(A<DomainId>._, A<long>._, ct))
            .Returns(Task.FromResult<ContentDomainObject.State>(null!));

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(unqiueId))
            .Returns(domainObject);

        sut = new ContentLoader(domainObjectFactory, domainObjectCache);
    }

    [Fact]
    public async Task Should_return_null_if_no_state_returned()
    {
        var content = (ContentDomainObject.State)null!;

        A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
            .Returns(content);

        Assert.Null(await sut.GetAsync(appId, id, 10, ct));
    }

    [Fact]
    public async Task Should_return_null_if_state_empty()
    {
        var content = new ContentDomainObject.State { Version = EtagVersion.Empty };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
            .Returns(content);

        Assert.Null(await sut.GetAsync(appId, id, 10, ct));
    }

    [Fact]
    public async Task Should_return_null_if_state_has_other_version()
    {
        var content = new ContentDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
            .Returns(content);

        Assert.Null(await sut.GetAsync(appId, id, 10, ct));
    }

    [Fact]
    public async Task Should_not_return_null_if_state_has_other_version_than_any()
    {
        var content = new ContentDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(EtagVersion.Any, ct))
            .Returns(content);

        var actual = await sut.GetAsync(appId, id, EtagVersion.Any, ct);

        Assert.Same(content, actual);
    }

    [Fact]
    public async Task Should_return_content_from_state()
    {
        var content = new ContentDomainObject.State { Version = 10 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, ct))
            .Returns(content);

        var actual = await sut.GetAsync(appId, id, 10, ct);

        Assert.Same(content, actual);
    }

    [Fact]
    public async Task Should_return_content_from_cache()
    {
        var content = new ContentDomainObject.State { Version = 10 };

        A.CallTo(() => domainObjectCache.GetAsync<ContentDomainObject.State>(DomainId.Combine(appId, id), 10, ct))
            .Returns(content);

        var actual = await sut.GetAsync(appId, id, 10, ct);

        Assert.Same(content, actual);

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(unqiueId))
            .MustNotHaveHappened();
    }
}
