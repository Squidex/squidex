// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class FieldRuleDto
    {
        /// <summary>
        /// The action to perform when the condition is met.
        /// </summary>
        [Required]
        public FieldRuleAction Action { get; set; }

        /// <summary>
        /// The field to update.
        /// </summary>
        [Required]
        public string Field { get; set; }

        /// <summary>
        /// The condition.
        /// </summary>
        public string? Condition { get; set; }

        public static FieldRuleDto FromFieldRule(FieldRule fieldRule)
        {
            return SimpleMapper.Map(fieldRule, new FieldRuleDto());
        }

        public FieldRuleCommand ToCommand()
        {
            return SimpleMapper.Map(this, new FieldRuleCommand());
        }
    }
}
