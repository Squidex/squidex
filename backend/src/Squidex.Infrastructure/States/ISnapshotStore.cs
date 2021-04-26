// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface ISnapshotStore<T>
    {
        Task WriteAsync(DomainId key, T value, long oldVersion, long newVersion);

        Task WriteManyAsync(IEnumerable<(DomainId Key, T Value, long Version)> snapshots);

        Task<(T Value, bool Valid, long Version)> ReadAsync(DomainId key);

        Task ClearAsync();

        Task RemoveAsync(DomainId key);

        Task ReadAllAsync(Func<T, long, Task> callback, CancellationToken ct = default);
    }
}
