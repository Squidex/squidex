// ==========================================================================
//  JsonAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class JsonAppEntity : JsonEntity, IAppEntity
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string PlanId { get; set; }

        [JsonProperty]
        public string Etag { get; set; }

        [JsonProperty]
        public string PlanOwner { get; set; }

        [JsonProperty]
        public AppClients Clients { get; set; }

        [JsonProperty]
        public AppContributors Contributors { get; set; }

        [JsonProperty]
        public LanguagesConfig LanguagesConfig { get; set; }
    }
}
