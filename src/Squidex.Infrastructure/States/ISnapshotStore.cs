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
    public interface ISnapshotStore
    {
        Task WriteAsync<T>(string key, T value, long oldVersion, long newVersion);

        Task<(T Value, long Version)> ReadAsync<T>(string key);
    }
}
