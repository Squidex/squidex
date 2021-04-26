// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.States
{
    [Flags]
    public enum PersistenceMode
    {
        EventSourcing = 1,
        Snapshots = 2,
        SnapshotsAndEventSourcing = 3
    }
}
