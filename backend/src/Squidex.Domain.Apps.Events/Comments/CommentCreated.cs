// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Comments;

[EventType(nameof(CommentCreated))]
public sealed class CommentCreated : AppEvent
{
    public static readonly NamedId<DomainId> NoApp = NamedId.Of(DomainId.NewGuid(), "no-app");

    public DomainId CommentsId { get; set; }

    public DomainId CommentId { get; set; }

    public string Text { get; set; }

    public string[]? Mentions { get; set; }

    public Uri? Url { get; set; }
}
