// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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

        public RuleTriggerDto Visit(AssetChangedTrigger trigger)
        {
            return SimpleMapper.Map(trigger, new AssetChangedTriggerDto());
        }

        public RuleTriggerDto Visit(ContentChangedTrigger trigger)
        {
            var schemas = trigger.Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchemaDto())).ToList();

            return new ContentChangedTriggerDto { Schemas = schemas, HandleAll = trigger.HandleAll };
        }
    }
}
