// ==========================================================================
//  NoopEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    [TypeName(nameof(NoopEvent))]
    public sealed class NoopEvent : IEvent
    {
    }
}
