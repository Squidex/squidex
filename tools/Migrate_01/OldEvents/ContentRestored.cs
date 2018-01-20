// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentRestored))]
    [Obsolete]
    public sealed class ContentRestored : ContentEvent, IMigratedEvent
    {
        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new ContentStatusChanged { Status = Status.Draft });
        }
    }
}
