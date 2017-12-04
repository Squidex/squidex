// ==========================================================================
//  IMigratedEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IMigratedEvent
    {
        IEvent Migrate();
    }
}
