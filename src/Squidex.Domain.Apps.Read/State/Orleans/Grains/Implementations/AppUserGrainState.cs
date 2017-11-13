// ==========================================================================
//  AppUserGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class AppUserGrainState
    {
        [JsonProperty]
        public HashSet<string> AppNames { get; set; } = new HashSet<string>();
    }
}
