// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Areas.Api.Controllers.Rules.Models.Triggers;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Converters
{
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
            return SimpleMapper.Map(trigger, new AssetChangedRuleTriggerDto());
        }

        public RuleTriggerDto Visit(CommentTrigger trigger)
        {
            return SimpleMapper.Map(trigger, new CommentRuleTriggerDto());
        }

        public RuleTriggerDto Visit(ManualTrigger trigger)
        {
            return SimpleMapper.Map(trigger, new ManualRuleTriggerDto());
        }

        public RuleTriggerDto Visit(SchemaChangedTrigger trigger)
        {
            return SimpleMapper.Map(trigger, new SchemaChangedRuleTriggerDto());
        }

        public RuleTriggerDto Visit(UsageTrigger trigger)
        {
            return SimpleMapper.Map(trigger, new UsageRuleTriggerDto());
        }

        public RuleTriggerDto Visit(ContentChangedTriggerV2 trigger)
        {
            var schemas = trigger.Schemas?.Select(ContentChangedRuleTriggerSchemaDto.FromTrigger).ToArray();

            return new ContentChangedRuleTriggerDto { Schemas = schemas, HandleAll = trigger.HandleAll };
        }
    }
}
