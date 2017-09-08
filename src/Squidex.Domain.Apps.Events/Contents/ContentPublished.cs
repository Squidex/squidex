// ==========================================================================
//  ContentPublished.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentPublished))]
    public sealed class ContentPublished : ContentEvent
    {
    }
}
