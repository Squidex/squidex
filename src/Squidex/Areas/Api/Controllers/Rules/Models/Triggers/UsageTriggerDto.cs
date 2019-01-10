// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class UsageTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// The number of monthly api calls.
        /// </summary>
        public int Limit { get; set; }

        public override RuleTrigger ToTrigger()
        {
            return SimpleMapper.Map(this, new UsageTrigger());
        }
    }
}
