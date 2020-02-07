// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentUpdateProposed))]
    [Obsolete]
    public sealed class ContentUpdateProposed : IMigrated<IEvent>
    {
        public IEvent Migrate()
        {
            return new NoopEvent();
        }
    }
}
