// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleCommandMiddleware(IDomainObjectFactory domainObjectFactory,
    IRuleEnricher ruleEnricher, IContextProvider contextProvider)
    : AggregateCommandMiddleware<RuleCommandBase, RuleDomainObject>(domainObjectFactory)
{
    protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        var payload = await base.EnrichResultAsync(context, result, ct);

        if (payload is Rule rule and not EnrichedRule)
        {
            payload = await ruleEnricher.EnrichAsync(rule, contextProvider.Context, ct);
        }

        return payload;
    }
}
