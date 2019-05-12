// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Services.Implementations
{
    public sealed class ConfigAppLimitsPlan : IAppLimitsPlan
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Costs { get; set; }

        public string YearlyCosts { get; set; }

        public string YearlyId { get; set; }

        public long MaxApiCalls { get; set; }

        public long MaxAssetSize { get; set; }

        public int MaxContributors { get; set; }

        public ConfigAppLimitsPlan Clone()
        {
            return (ConfigAppLimitsPlan)MemberwiseClone();
        }
    }
}