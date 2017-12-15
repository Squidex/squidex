// ==========================================================================
//  PersistenceMode.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.States
{
    public enum PersistenceMode
    {
        EventSourcing,
        Snapshots,
        SnapshotsAndEventSourcing
    }
}
