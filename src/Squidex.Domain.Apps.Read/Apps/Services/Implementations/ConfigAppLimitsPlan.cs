// ==========================================================================
//  ConfigAppLimitsPlan.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Apps.Services.Implementations
{
    public sealed class ConfigAppLimitsPlan : IAppLimitsPlan
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Costs { get; set; }

        public long MaxApiCalls { get; set; }

        public long MaxAssetSize { get; set; }

        public int MaxContributors { get; set; }

        public ConfigAppLimitsPlan Clone()
        {
            return (ConfigAppLimitsPlan)MemberwiseClone();
        }
    }
}