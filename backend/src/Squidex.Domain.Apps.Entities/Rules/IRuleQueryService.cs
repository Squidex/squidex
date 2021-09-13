// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public interface IRuleQueryService
    {
        Task<IReadOnlyList<IEnrichedRuleEntity>> QueryAsync(Context context,
            CancellationToken ct = default);
    }
}
