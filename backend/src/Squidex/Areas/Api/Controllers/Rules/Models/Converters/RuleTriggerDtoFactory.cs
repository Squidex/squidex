// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Rules.Models.Triggers;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Converters;

public sealed class RuleTriggerDtoFactory : IRuleTriggerVisitor<RuleTriggerDto>
{
    private static readonly RuleTriggerDtoFactory Instance = new RuleTriggerDtoFactory();

    private RuleTriggerDtoFactory()
    {
    }

    public static RuleTriggerDto Create(RuleTrigger properties)
    {
        return properties.Accept(Instance);
    }

    public RuleTriggerDto Visit(AssetChangedTriggerV2 trigger)
    {
        return AssetChangedRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(CommentTrigger trigger)
    {
        return CommentRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(ManualTrigger trigger)
    {
        return ManualRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(SchemaChangedTrigger trigger)
    {
        return SchemaChangedRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(UsageTrigger trigger)
    {
        return UsageRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(ContentChangedTriggerV2 trigger)
    {
        return ContentChangedRuleTriggerDto.FromDomain(trigger);
    }
}
