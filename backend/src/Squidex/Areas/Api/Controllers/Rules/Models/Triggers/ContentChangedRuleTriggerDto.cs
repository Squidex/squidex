// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers;

public sealed class ContentChangedRuleTriggerDto : RuleTriggerDto
{
    /// <summary>
    /// The schema settings.
    /// </summary>
    public ReadonlyList<SchemaCondition>? Schemas { get; set; }

    /// <summary>
    /// The schema references.
    /// </summary>
    public ReadonlyList<SchemaCondition>? ReferencedSchemas { get; set; }

    /// <summary>
    /// Determines whether the trigger should handle all content changes events.
    /// </summary>
    public bool HandleAll { get; set; }

    public static ContentChangedRuleTriggerDto FromDomain(ContentChangedTriggerV2 trigger)
    {
        return SimpleMapper.Map(trigger, new ContentChangedRuleTriggerDto());
    }

    public override RuleTrigger ToTrigger()
    {
        return SimpleMapper.Map(this, new ContentChangedTriggerV2());
    }
}
