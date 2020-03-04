// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentUpdateProposed))]
    [Obsolete]
    public sealed class ContentUpdateProposed : SquidexEvent, IMigrated<IEvent>
    {
        public NamedContentData Data { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new ContentUpdated());
        }
    }
}
