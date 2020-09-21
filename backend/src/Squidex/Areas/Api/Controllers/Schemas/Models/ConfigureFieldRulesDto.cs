// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class ConfigureFieldRulesDto
    {
        /// <summary>
        /// The field rules to configure.
        /// </summary>
        public FieldRuleDto[]? FieldRules { get; set; }

        public ConfigureFieldRules ToCommand()
        {
            return new ConfigureFieldRules
            {
                FieldRules = FieldRules?.Select(x => x.ToCommand()).ToArray()
            };
        }
    }
}
