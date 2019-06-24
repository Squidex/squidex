// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using ContentCreatedV2 = Squidex.Domain.Apps.Events.Contents.ContentCreated;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentCreated))]
    [Obsolete]
    public sealed class ContentCreated : ContentEvent, IMigrated<IEvent>
    {
        public Status Status { get; set; }

        public NamedContentData Data { get; set; }

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
