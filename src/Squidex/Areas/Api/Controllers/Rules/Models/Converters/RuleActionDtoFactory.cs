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

        public RuleActionDto Visit(AlgoliaAction action)
        {
            return SimpleMapper.Map(action, new AlgoliaActionDto());
        }

        public RuleActionDto Visit(AzureQueueAction action)
        {
            return SimpleMapper.Map(action, new AzureQueueActionDto());
        }

        public RuleActionDto Visit(ElasticSearchAction action)
        {
            return SimpleMapper.Map(action, new ElasticSearchActionDto());
        }

        public RuleActionDto Visit(FastlyAction action)
        {
            return SimpleMapper.Map(action, new FastlyActionDto());
        }

        public RuleActionDto Visit(MediumAction action)
        {
            return SimpleMapper.Map(action, new MediumActionDto());
        }

        public RuleActionDto Visit(SlackAction action)
        {
            return SimpleMapper.Map(action, new SlackActionDto());
        }

        public RuleActionDto Visit(WebhookAction action)
        {
            return SimpleMapper.Map(action, new WebhookActionDto());
        }
    }
}
