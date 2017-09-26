// ==========================================================================
//  ContentStatusChanged.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentStatusChanged))]
    public sealed class ContentStatusChanged : ContentEvent
    {
        public Status Status { get; set; }
    }
}
