// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class ContentChangedRuleTriggerDto : RuleTriggerDto
    {
        /// <summary>
        /// The schema settings.
        /// </summary>
        [LocalizedRequired]
        public ContentChangedRuleTriggerSchemaDto[] Schemas { get; set; }

        /// <summary>
        /// Determines whether the trigger should handle all content changes events.
        /// </summary>
        public bool HandleAll { get; set; }

        public override RuleTrigger ToTrigger()
        {
            var schemas = Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchemaV2())).ToReadOnlyCollection();

            return new ContentChangedTriggerV2 { HandleAll = HandleAll, Schemas = schemas };
        }
    }
}
