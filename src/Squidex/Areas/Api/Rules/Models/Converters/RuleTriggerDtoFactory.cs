// ==========================================================================
//  RuleTriggerDtoFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        public RuleTriggerDto Visit(ContentChangedTrigger trigger)
        {
            return new ContentChangedTriggerDto
            {
                Schemas = trigger.Schemas.Select(x => SimpleMapper.Map(x, new ContentChangedTriggerSchemaDto())).ToList()
            };
        }
    }
}
