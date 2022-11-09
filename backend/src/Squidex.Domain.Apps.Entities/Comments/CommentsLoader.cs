// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Comments.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Comments;

public sealed class CommentsLoader : ICommentsLoader
{
    private readonly IDomainObjectFactory domainObjectFactory;

    public CommentsLoader(IDomainObjectFactory domainObjectFactory)
    {
        this.domainObjectFactory = domainObjectFactory;
    }

    public async Task<CommentsResult> GetCommentsAsync(DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        var stream = domainObjectFactory.Create<CommentsStream>(id);

        await stream.LoadAsync(ct);

        return stream.GetComments(version);
    }
}
