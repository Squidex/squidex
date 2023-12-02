// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleEnricher
{
    Task<EnrichedRule> EnrichAsync(Rule rule, Context context,
        CancellationToken ct);

    Task<IReadOnlyList<EnrichedRule>> EnrichAsync(IEnumerable<Rule> rules, Context context,
        CancellationToken ct);
}
