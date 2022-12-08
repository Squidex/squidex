// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Comments;

public abstract class CommentsEvent : AppEvent
{
    public DomainId CommentsId { get; set; }

    public DomainId CommentId { get; set; }
}
