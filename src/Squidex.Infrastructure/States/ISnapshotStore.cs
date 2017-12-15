// ==========================================================================
//  ISnapshotStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface ISnapshotStore<T, TKey>
    {
        Task WriteAsync(TKey key, T value, long oldVersion, long newVersion);

        Task<(T Value, long Version)> ReadAsync(TKey key);
    }
}
