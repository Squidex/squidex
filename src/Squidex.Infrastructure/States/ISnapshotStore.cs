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
        Task WriteAsync<T>(string key, T value, string oldEtag, string newEtag);

        Task<(T Value, string Etag)> ReadAsync<T>(string key);
    }
}
