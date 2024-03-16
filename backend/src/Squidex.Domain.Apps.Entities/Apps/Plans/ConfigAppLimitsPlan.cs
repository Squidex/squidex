// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class ConfigAppLimitsPlan : IAppLimitsPlan
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Costs { get; set; }

        public string? ConfirmText { get; set; }

        public string? YearlyCosts { get; set; }

        public string? YearlyId { get; set; }

        public string? YearlyConfirmText { get; set; }

        public long BlockingApiCalls { get; set; }

        public long MaxApiCalls { get; set; }

        public long MaxApiBytes { get; set; }

        public long MaxAssetSize { get; set; }

        public int MaxContributors { get; set; }

        public bool IsFree { get; set; }

        public ConfigAppLimitsPlan Clone()
        {
            return (ConfigAppLimitsPlan)MemberwiseClone();
        }
    }
}