// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    [JsonSchema("ContentChanged")]
    public sealed class ContentChangedTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// The schema settings.
        /// </summary>
        [Required]
        public List<ContentChangedTriggerSchemaDto> Schemas { get; set; }

        /// <summary>
        /// Determines whether the trigger should handle all content changes events.
        /// </summary>
        public bool HandleAll { get; set; }

        public override RuleTrigger ToTrigger()
        {
            var schemas = Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchema())).ToImmutableList();

            return new ContentChangedTrigger { HandleAll = HandleAll, Schemas = schemas };
        }
    }
}
