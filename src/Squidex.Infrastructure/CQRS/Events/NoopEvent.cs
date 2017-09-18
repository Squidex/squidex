// ==========================================================================
//  NoopEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    [TypeName(nameof(NoopEvent))]
    public sealed class NoopEvent : IEvent
    {
    }
}
