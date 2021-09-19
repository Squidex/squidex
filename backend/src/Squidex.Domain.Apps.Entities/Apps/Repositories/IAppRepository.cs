// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<Dictionary<string, DomainId>> QueryIdsAsync(string contributorId,
            CancellationToken ct = default);

        Task<Dictionary<string, DomainId>> QueryIdsAsync(IEnumerable<string> names,
            CancellationToken ct = default);
    }
}
