// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public abstract class EnrichedEvent
    {
        public NamedId<Guid> AppId { get; set; }

        public RefToken Actor { get; set; }

        public Instant Timestamp { get; set; }

        public long Version { get; set; }

        [JsonIgnore]
        public abstract Guid AggregateId { get; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public IUser User { get; set; }
    }
}
