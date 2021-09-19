// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class RuleCommandMiddleware : GrainCommandMiddleware<RuleCommand, IRuleGrain>
    {
        private readonly IRuleEnricher ruleEnricher;
        private readonly IContextProvider contextProvider;

        public RuleCommandMiddleware(IGrainFactory grainFactory, IRuleEnricher ruleEnricher, IContextProvider contextProvider)
            : base(grainFactory)
        {
            this.ruleEnricher = ruleEnricher;

            this.contextProvider = contextProvider;
        }

        protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            var payload = await base.EnrichResultAsync(context, result);

            if (payload is IRuleEntity rule && payload is not IEnrichedRuleEntity)
            {
                payload = await ruleEnricher.EnrichAsync(rule, contextProvider.Context, default);
            }

            return payload;
        }
    }
}
