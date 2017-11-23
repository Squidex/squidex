// ==========================================================================
//  JsonAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public sealed class JsonAppEntity : JsonEntity<JsonAppEntity>, IAppEntity
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
        public AppClients Clients { get; set; } = AppClients.Empty;

        [JsonProperty]
        public AppContributors Contributors { get; set; } = AppContributors.Empty;

        [JsonProperty]
        public LanguagesConfig LanguagesConfig { get; set; }
    }
}
