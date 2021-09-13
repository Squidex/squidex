// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public interface IContentEnricher
    {
        Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, bool cloneData, Context context,
            CancellationToken ct);

        Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context,
            CancellationToken ct);
    }
}
