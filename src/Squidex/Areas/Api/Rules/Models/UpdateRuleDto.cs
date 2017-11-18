// ==========================================================================
//  UpdateRuleDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class UpdateRuleDto
    {
        /// <summary>
        /// The trigger properties.
        /// </summary>
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        public RuleActionDto Action { get; set; }
    }
}
