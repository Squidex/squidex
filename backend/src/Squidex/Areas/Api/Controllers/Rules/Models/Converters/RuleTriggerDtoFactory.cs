// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Rules.Models.Triggers;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Converters;

public sealed class RuleTriggerDtoFactory : IRuleTriggerVisitor<RuleTriggerDto, None>
{
    private static readonly RuleTriggerDtoFactory Instance = new RuleTriggerDtoFactory();

    private RuleTriggerDtoFactory()
    {
    }

    public static RuleTriggerDto Create(RuleTrigger properties)
    {
        return properties.Accept(Instance, None.Value);
    }

    public RuleTriggerDto Visit(AssetChangedTriggerV2 trigger, None args)
    {
        return AssetChangedRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(CommentTrigger trigger, None args)
    {
        return CommentRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(CronJobTrigger trigger, None args)
    {
        return CronJobRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(ManualTrigger trigger, None args)
    {
        return ManualRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(SchemaChangedTrigger trigger, None args)
    {
        return SchemaChangedRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(UsageTrigger trigger, None args)
    {
        return UsageRuleTriggerDto.FromDomain(trigger);
    }

    public RuleTriggerDto Visit(ContentChangedTriggerV2 trigger, None args)
    {
        return ContentChangedRuleTriggerDto.FromDomain(trigger);
    }
}
