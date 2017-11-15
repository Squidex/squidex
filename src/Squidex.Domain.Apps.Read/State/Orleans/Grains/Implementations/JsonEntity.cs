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
using Orleans.Concurrency;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    [Immutable]
    public abstract class JsonEntity<T> : Cloneable<T>, IUpdateableEntityWithVersion where T : Cloneable
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Instant Created { get; set; }

        [JsonProperty]
        public Instant LastModified { get; set; }

        [JsonProperty]
        public long Version { get; set; }

        public T Clone()
        {
            return Clone(x => { });
        }
    }
}
