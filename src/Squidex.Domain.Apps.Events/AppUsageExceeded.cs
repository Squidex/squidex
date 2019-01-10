// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events
{
    [EventType(nameof(AppUsageExceeded))]
    public sealed class AppUsageExceeded : AppEvent
    {
        public long Current { get; set; }

        public long Limit { get; set; }
    }
}
