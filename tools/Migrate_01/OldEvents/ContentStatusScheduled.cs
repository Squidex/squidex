// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using ContentStatusScheduledV2 = Squidex.Domain.Apps.Events.Contents.ContentStatusScheduled;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentStatusScheduled))]
    [Obsolete]
    public sealed class ContentStatusScheduled : ContentEvent, IMigrated<IEvent>
    {
        public Status Status { get; set; }

        public Instant DueTime { get; set; }

        public IEvent Migrate()
        {
            var migrated = SimpleMapper.Map(this, new ContentStatusScheduledV2());

            if (migrated.Status == default)
            {
                migrated.Status = Status.Draft;
            }

            return this;
        }
    }
}
