// ==========================================================================
//  AppUserGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed class AppUserGrainState : Cloneable<AppUserGrainState>
    {
        [JsonProperty]
        public ImmutableHashSet<string> AppNames { get; set; } = ImmutableHashSet<string>.Empty;

        public AppUserGrainState AddApp(string appName)
        {
            return Clone(c => c.AppNames = c.AppNames.Add(appName));
        }

        public AppUserGrainState RemoveApp(string appName)
        {
            return Clone(c => c.AppNames = c.AppNames.Remove(appName));
        }
    }
}
