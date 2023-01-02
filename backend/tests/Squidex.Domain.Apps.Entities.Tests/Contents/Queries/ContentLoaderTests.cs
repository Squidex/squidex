// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentLoaderTests : GivenContext
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly ContentDomainObject domainObject = A.Fake<ContentDomainObject>();
    private readonly DomainId id = DomainId.NewGuid();
    private readonly DomainId unqiueId;
    private readonly ContentLoader sut;

    public ContentLoaderTests()
    {
        unqiueId = DomainId.Combine(AppId.Id, id);

        A.CallTo(() => domainObjectCache.GetAsync<ContentDomainObject.State>(A<DomainId>._, A<long>._, CancellationToken))
            .Returns(Task.FromResult<ContentDomainObject.State>(null!));

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(unqiueId))
            .Returns(domainObject);

        sut = new ContentLoader(domainObjectFactory, domainObjectCache);
    }

    [Fact]
    public async Task Should_return_null_if_no_state_returned()
    {
        var content = (ContentDomainObject.State)null!;

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(content);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_return_null_if_state_empty()
    {
        var content = new ContentDomainObject.State { Version = EtagVersion.Empty };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(content);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_return_null_if_state_has_other_version()
    {
        var content = new ContentDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(content);

        Assert.Null(await sut.GetAsync(AppId.Id, id, 10, CancellationToken));
    }

    [Fact]
    public async Task Should_not_return_null_if_state_has_other_version_than_any()
    {
        var content = new ContentDomainObject.State { Version = 5 };

        A.CallTo(() => domainObject.GetSnapshotAsync(EtagVersion.Any, CancellationToken))
            .Returns(content);

        var actual = await sut.GetAsync(AppId.Id, id, EtagVersion.Any, CancellationToken);

        Assert.Same(content, actual);
    }

    [Fact]
    public async Task Should_return_content_from_state()
    {
        var content = new ContentDomainObject.State { Version = 10 };

        A.CallTo(() => domainObject.GetSnapshotAsync(10, CancellationToken))
            .Returns(content);

        var actual = await sut.GetAsync(AppId.Id, id, 10, CancellationToken);

        Assert.Same(content, actual);
    }

    [Fact]
    public async Task Should_return_content_from_cache()
    {
        var content = new ContentDomainObject.State { Version = 10 };

        A.CallTo(() => domainObjectCache.GetAsync<ContentDomainObject.State>(DomainId.Combine(AppId.Id, id), 10, CancellationToken))
            .Returns(content);

        var actual = await sut.GetAsync(AppId.Id, id, 10, CancellationToken);

        Assert.Same(content, actual);

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(unqiueId))
            .MustNotHaveHappened();
    }
}
