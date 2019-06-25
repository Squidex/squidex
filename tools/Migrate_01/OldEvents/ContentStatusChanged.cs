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
using ContentStatusChangedV2 = Squidex.Domain.Apps.Events.Contents.ContentStatusChanged;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ContentStatusChanged))]
    [Obsolete]
    public sealed class ContentStatusChanged : ContentEvent, IMigrated<IEvent>
    {
        public string Change { get; set; }

        public Status Status { get; set; }

        public IEvent Migrate()
        {
            var migrated = SimpleMapper.Map(this, new ContentStatusChangedV2());

            if (migrated.Status == default)
            {
                migrated.Status = Status.Draft;
            }

            if (Enum.TryParse<StatusChange>(Change, out var result))
            {
                migrated.Change = result;
            }
            else
            {
                migrated.Change = StatusChange.Change;
            }

            return migrated;
        }
    }
}
