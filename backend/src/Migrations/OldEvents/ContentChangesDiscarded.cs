// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents
{
    [EventType(nameof(ContentChangesDiscarded))]
    [Obsolete("New Event introduced")]
    public sealed class ContentChangesDiscarded : ContentEvent, IMigrated<IEvent>
    {
        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new ContentDraftDeleted());
        }
    }
}
