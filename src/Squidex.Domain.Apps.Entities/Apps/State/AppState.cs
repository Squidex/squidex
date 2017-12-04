// ==========================================================================
//  AppState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.State
{
    public sealed class AppState : DomainObjectState<AppState>, IAppEntity
    {
        private static readonly LanguagesConfig English = LanguagesConfig.Build(Language.EN);

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public AppPlan Plan { get; set; }

        [JsonProperty]
        public AppClients Clients { get; set; } = AppClients.Empty;

        [JsonProperty]
        public AppContributors Contributors { get; set; } = AppContributors.Empty;

        [JsonProperty]
        public LanguagesConfig LanguagesConfig { get; set; } = English;
    }
}
