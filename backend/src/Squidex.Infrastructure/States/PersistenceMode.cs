// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
