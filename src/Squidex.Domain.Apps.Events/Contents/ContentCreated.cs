// ==========================================================================
//  ContentCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentCreated))]
    public sealed class ContentCreated : ContentEvent
    {
        public NamedContentData Data { get; set; }
    }
}
