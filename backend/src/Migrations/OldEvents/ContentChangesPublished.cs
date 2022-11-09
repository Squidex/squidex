// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using ContentStatusChangedV2 = Squidex.Domain.Apps.Events.Contents.ContentStatusChanged;

namespace Migrations.OldEvents;

[EventType(nameof(ContentChangesPublished))]
[Obsolete("New Event introduced")]
public sealed class ContentChangesPublished : ContentEvent, IMigrated<IEvent>
{
    public IEvent Migrate()
    {
        return SimpleMapper.Map(this, new ContentStatusChangedV2
        {
            Status = Status.Published,
            Change = StatusChange.Published
        });
    }
}
