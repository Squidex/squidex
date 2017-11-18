// ==========================================================================
//  RuleConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Write.Rules.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Rules.Models.Converters
{
    public static class RuleConverter
    {
        public static RuleDto ToModel(this IRuleEntity entity)
        {
            var dto = new RuleDto();

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.RuleDef, dto);

            if (entity.RuleDef.Trigger != null)
            {
                dto.Trigger = RuleTriggerDtoFactory.Create(entity.RuleDef.Trigger);
            }

            if (entity.RuleDef.Action != null)
            {
                dto.Action = RuleActionDtoFactory.Create(entity.RuleDef.Action);
            }

            return dto;
        }

        public static UpdateRule ToCommand(this UpdateRuleDto dto, Guid id)
        {
            var command = new UpdateRule { RuleId = id };

            if (dto.Action != null)
            {
                command.Action = dto.Action.ToAction();
            }

            if (dto.Trigger != null)
            {
                command.Trigger = dto.Trigger.ToTrigger();
            }

            return command;
        }

        public static CreateRule ToCommand(this CreateRuleDto dto)
        {
            var command = new CreateRule();

            if (dto.Action != null)
            {
                command.Action = dto.Action.ToAction();
            }

            if (dto.Trigger != null)
            {
                command.Trigger = dto.Trigger.ToTrigger();
            }

            return command;
        }
    }
}
