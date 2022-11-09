// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public interface IDomainObjectCache
{
    Task<T> GetAsync<T>(DomainId id, long version,
        CancellationToken ct = default);

    Task SetAsync<T>(DomainId id, long version, T snapshot,
        CancellationToken ct = default);
}
