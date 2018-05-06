// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface ISnapshotStore<T, TKey>
    {
        Task WriteAsync(TKey key, T value, long oldVersion, long newVersion);

        Task<(T Value, long Version)> ReadAsync(TKey key);

        Task ClearAsync();

        Task ReadAllAsync(Func<T, long, Task> callback);
    }
}
