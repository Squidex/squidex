// ==========================================================================
//  ContentArchived.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentArchived))]
    public sealed class ContentArchived : ContentEvent
    {
    }
}
