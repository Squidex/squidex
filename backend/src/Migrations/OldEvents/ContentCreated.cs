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
using ContentCreatedV2 = Squidex.Domain.Apps.Events.Contents.ContentCreated;

namespace Migrations.OldEvents
{
    [EventType(nameof(ContentCreated))]
    [Obsolete("New Event introduced")]
    public sealed class ContentCreated : ContentEvent, IMigrated<IEvent>
    {
        public Status Status { get; set; }

        public ContentData Data { get; set; }

        public IEvent Migrate()
        {
            var migrated = SimpleMapper.Map(this, new ContentCreatedV2());

            if (migrated.Status == default)
            {
                migrated.Status = Status.Draft;
            }

            return migrated;
        }
    }
}
