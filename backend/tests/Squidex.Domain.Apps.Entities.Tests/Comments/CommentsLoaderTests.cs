// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Comments.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Comments;

public sealed class CommentsLoaderTests : GivenContext
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly CommentsLoader sut;

    public CommentsLoaderTests()
    {
        sut = new CommentsLoader(domainObjectFactory);
    }

    [Fact]
    public async Task Should_get_comments_from_domain_object()
    {
        var commentsId = DomainId.NewGuid();
        var comments = new CommentsResult();

        var domainObject = A.Fake<CommentsStream>();

        A.CallTo(() => domainObjectFactory.Create<CommentsStream>(commentsId))
            .Returns(domainObject);

        A.CallTo(() => domainObject.GetComments(11))
            .Returns(comments);

        var actual = await sut.GetCommentsAsync(commentsId, 11, CancellationToken);

        Assert.Same(comments, actual);

        A.CallTo(() => domainObject.LoadAsync(CancellationToken))
            .MustHaveHappened();
    }
}
