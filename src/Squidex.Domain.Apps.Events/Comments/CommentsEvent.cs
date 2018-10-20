// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Comments
{
    public abstract class CommentsEvent : AppEvent
    {
        public Guid CommentsId { get; set; }
    }
}
