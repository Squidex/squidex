// ==========================================================================
//  ContentUnpublished.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentUnpublished))]
    public sealed class ContentUnpublished : ContentEvent
    {
    }
}
