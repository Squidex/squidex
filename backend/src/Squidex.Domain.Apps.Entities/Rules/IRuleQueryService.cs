// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleQueryService
{
    Task<IReadOnlyList<EnrichedRule>> QueryAsync(Context context,
        CancellationToken ct = default);
}
