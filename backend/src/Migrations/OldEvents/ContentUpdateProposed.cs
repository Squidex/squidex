// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents
{
    [EventType(nameof(ContentUpdateProposed))]
    [Obsolete("New Event introduced")]
    public sealed class ContentUpdateProposed : ContentEvent, IMigrated<IEvent>
    {
        public ContentData Data { get; set; }

        public IEvent Migrate()
        {
            var migrated = SimpleMapper.Map(this, new ContentDraftCreated());

            migrated.MigratedData = Data;

            if (migrated.Status == default)
            {
                migrated.Status = Status.Draft;
            }

            return migrated;
        }
    }
}
