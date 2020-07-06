// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events
{
    [EventType(nameof(AppUsageExceeded))]
    public sealed class AppUsageExceeded : AppEvent
    {
        public long CallsCurrent { get; set; }

        public long CallsLimit { get; set; }

        public DomainId RuleId { get; set; }
    }
}
