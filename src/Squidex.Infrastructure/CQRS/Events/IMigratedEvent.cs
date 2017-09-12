// ==========================================================================
//  IMigratedEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IMigratedEvent
    {
        IEvent Migrate();
    }
}
