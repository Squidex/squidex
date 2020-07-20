// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public abstract class EnrichedEvent
    {
        public NamedId<DomainId> AppId { get; set; }

        public Instant Timestamp { get; set; }

        public string Name { get; set; }

        public long Version { get; set; }

        public abstract long Partition { get; }
    }
}
