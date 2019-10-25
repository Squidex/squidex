// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public abstract class EnrichedEvent
    {
        public NamedId<Guid> AppId { get; set; }

        public Instant Timestamp { get; set; }

        public string Name { get; set; }

        public long Version { get; set; }

        public abstract long Partition { get; }
    }
}
