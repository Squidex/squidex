﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Comments
{
    [EventType(nameof(CommentDeleted))]
    public sealed class CommentDeleted : CommentsEvent
    {
        public Guid CommentId { get; set; }
    }
}
