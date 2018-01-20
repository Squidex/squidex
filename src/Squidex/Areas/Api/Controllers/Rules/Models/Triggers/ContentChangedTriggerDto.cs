﻿// ==========================================================================
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

        public override RuleTrigger ToTrigger()
        {
            return new ContentChangedTrigger
            {
                Schemas = Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchema())).ToImmutableList()
            };
        }
    }
}
