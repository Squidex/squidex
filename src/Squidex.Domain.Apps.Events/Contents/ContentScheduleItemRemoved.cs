// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentStatusChanged))]
    public sealed class ContentScheduleItemRemoved : ContentEvent
    {
        public Guid ScheduleItemId { get; set; }
    }
}
