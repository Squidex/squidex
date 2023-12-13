// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Repositories;

public interface IAppRepository
{
    Task<List<App>> QueryAllAsync(string contributorId, IEnumerable<string> names,
        CancellationToken ct = default);

    Task<List<App>> QueryAllAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<App?> FindAsync(DomainId id,
        CancellationToken ct = default);

    Task<App?> FindAsync(string name,
        CancellationToken ct = default);
}
