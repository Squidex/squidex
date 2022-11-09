// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleEnricher
{
    Task<IEnrichedRuleEntity> EnrichAsync(IRuleEntity rule, Context context,
        CancellationToken ct);

    Task<IReadOnlyList<IEnrichedRuleEntity>> EnrichAsync(IEnumerable<IRuleEntity> rules, Context context,
        CancellationToken ct);
}
