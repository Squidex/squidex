// ==========================================================================
//  CreateRuleDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Rules.Models
{
    public sealed class CreateRuleDto
    {
        /// <summary>
        /// The trigger properties.
        /// </summary>
        [Required]
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [Required]
        public RuleActionDto Action { get; set; }
    }
}
