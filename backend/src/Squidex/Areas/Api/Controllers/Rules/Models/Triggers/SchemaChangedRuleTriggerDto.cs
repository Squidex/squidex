// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers;

public sealed class SchemaChangedRuleTriggerDto : RuleTriggerDto
{
    /// <summary>
    /// Javascript condition when to trigger.
    /// </summary>
    public string? Condition { get; set; }

    public static SchemaChangedRuleTriggerDto FromDomain(SchemaChangedTrigger trigger)
    {
        return SimpleMapper.Map(trigger, new SchemaChangedRuleTriggerDto());
    }

    public override RuleTrigger ToTrigger()
    {
        return SimpleMapper.Map(this, new SchemaChangedTrigger());
    }
}
