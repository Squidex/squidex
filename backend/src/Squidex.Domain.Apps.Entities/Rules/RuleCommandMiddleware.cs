// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleCommandMiddleware : AggregateCommandMiddleware<RuleCommandBase, RuleDomainObject>
{
    private readonly IRuleEnricher ruleEnricher;
    private readonly IContextProvider contextProvider;

    public RuleCommandMiddleware(IDomainObjectFactory domainObjectFactory,
        IRuleEnricher ruleEnricher, IContextProvider contextProvider)
        : base(domainObjectFactory)
    {
        this.ruleEnricher = ruleEnricher;
        this.contextProvider = contextProvider;
    }

    protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        var payload = await base.EnrichResultAsync(context, result, ct);

        if (payload is IRuleEntity rule and not IEnrichedRuleEntity)
        {
            payload = await ruleEnricher.EnrichAsync(rule, contextProvider.Context, ct);
        }

        return payload;
    }
}
