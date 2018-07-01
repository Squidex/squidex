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
    public class EnrichedEvent
    {
        public Guid AggregateId { get; set; }

        public NamedId<Guid> AppId { get; set; }

        public RefToken Actor { get; set; }

        public Instant Timestamp { get; set; }

        public string Name { get; set; }
    }
}
