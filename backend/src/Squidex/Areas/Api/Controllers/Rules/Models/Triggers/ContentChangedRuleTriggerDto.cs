// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Collections;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class ContentChangedRuleTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// The schema settings.
        /// </summary>
        public ContentChangedRuleTriggerSchemaDto[]? Schemas { get; set; }

        /// <summary>
        /// Determines whether the trigger should handle all content changes events.
        /// </summary>
        public bool HandleAll { get; set; }

        public override RuleTrigger ToTrigger()
        {
            var schemas = Schemas?.Select(x => x.ToTrigger()).ToImmutableList();

            return new ContentChangedTriggerV2 { HandleAll = HandleAll, Schemas = schemas };
        }
    }
}
