// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class UsageRuleTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// The number of monthly api calls.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The number of days to check or null for the current month.
        /// </summary>
        [Range(1, 30)]
        public int? NumDays { get; set; }

        public override RuleTrigger ToTrigger()
        {
            return SimpleMapper.Map(this, new UsageTrigger());
        }
    }
}
