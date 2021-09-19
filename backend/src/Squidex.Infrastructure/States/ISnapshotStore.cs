// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface ISnapshotStore<T>
    {
        Task WriteAsync(DomainId key, T value, long oldVersion, long newVersion,
            CancellationToken ct = default);

        Task WriteManyAsync(IEnumerable<(DomainId Key, T Value, long Version)> snapshots,
            CancellationToken ct = default);

        Task<(T Value, bool Valid, long Version)> ReadAsync(DomainId key,
            CancellationToken ct = default);

        Task ClearAsync(
            CancellationToken ct = default);

        Task RemoveAsync(DomainId key,
            CancellationToken ct = default);

        IAsyncEnumerable<(T State, long Version)> ReadAllAsync(
            CancellationToken ct = default);
    }
}
