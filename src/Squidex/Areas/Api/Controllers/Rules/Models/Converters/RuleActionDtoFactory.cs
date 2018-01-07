// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Rules.Models.Actions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Converters
{
    public sealed class RuleActionDtoFactory : IRuleActionVisitor<RuleActionDto>
    {
        private static readonly RuleActionDtoFactory Instance = new RuleActionDtoFactory();

        private RuleActionDtoFactory()
        {
        }

        public static RuleActionDto Create(RuleAction properties)
        {
            return properties.Accept(Instance);
        }

        public RuleActionDto Visit(WebhookAction action)
        {
            return SimpleMapper.Map(action, new WebhookActionDto());
        }
    }
}
