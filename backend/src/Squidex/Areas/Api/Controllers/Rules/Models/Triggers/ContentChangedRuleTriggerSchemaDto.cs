// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers;

public sealed class ContentChangedRuleTriggerSchemaDto
{
    /// <summary>
    /// The ID of the schema.
    /// </summary>
    public DomainId SchemaId { get; set; }

    /// <summary>
    /// Javascript condition when to trigger.
    /// </summary>
    public string? Condition { get; set; }

    public ContentChangedTriggerSchemaV2 ToTrigger()
    {
        return SimpleMapper.Map(this, new ContentChangedTriggerSchemaV2());
    }

    public static ContentChangedRuleTriggerSchemaDto FromDomain(ContentChangedTriggerSchemaV2 trigger)
    {
        var result = SimpleMapper.Map(trigger, new ContentChangedRuleTriggerSchemaDto());

        return result;
    }
}
