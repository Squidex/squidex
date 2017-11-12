// ==========================================================================
//  JsonEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public abstract class JsonEntity
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Instant Created { get; set; }

        [JsonProperty]
        public Instant LastModified { get; set; }

        [JsonProperty]
        public long Version { get; set; }
    }
}
