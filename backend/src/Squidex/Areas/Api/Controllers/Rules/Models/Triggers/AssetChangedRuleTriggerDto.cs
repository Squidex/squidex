﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class AssetChangedRuleTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// Javascript condition when to trigger.
        /// </summary>
        public string Condition { get; set; }

        public override RuleTrigger ToTrigger()
        {
            return SimpleMapper.Map(this, new AssetChangedTriggerV2());
        }
    }
}
