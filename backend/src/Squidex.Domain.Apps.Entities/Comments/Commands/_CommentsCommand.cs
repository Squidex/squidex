// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Comments.Commands;

public abstract class CommentCommand : CommentsCommand
{
    public DomainId CommentId { get; set; }
}

public abstract class CommentsCommand : CommentsCommandBase
{
    public static readonly NamedId<DomainId> NoApp = NamedId.Of(DomainId.Empty, "none");

    public DomainId CommentsId { get; set; }

    public override DomainId AggregateId
    {
        get
        {
            if (AppId.Id == default)
            {
                return CommentsId;
            }
            else
            {
                return DomainId.Combine(AppId, CommentsId);
            }
        }
    }
}

// This command is needed as marker for middlewares.
public abstract class CommentsCommandBase : SquidexCommand, IAppCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public abstract DomainId AggregateId { get; }
}
