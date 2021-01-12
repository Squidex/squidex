// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure;
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
            Guard.NotNull(ruleEnricher, nameof(ruleEnricher));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.ruleEnricher = ruleEnricher;

            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await base.HandleAsync(context, next);

            if (context.PlainResult is IRuleEntity rule && NotEnriched(context))
            {
                var enriched = await ruleEnricher.EnrichAsync(rule, contextProvider.Context);

                context.Complete(enriched);
            }
        }

        private static bool NotEnriched(CommandContext context)
        {
            return !(context.PlainResult is IEnrichedRuleEntity);
        }
    }
}
