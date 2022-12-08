// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.States;

public interface ISnapshotStore<T>
{
    Task WriteAsync(SnapshotWriteJob<T> job,
        CancellationToken ct = default);

    Task WriteManyAsync(IEnumerable<SnapshotWriteJob<T>> jobs,
        CancellationToken ct = default);

    Task<SnapshotResult<T>> ReadAsync(DomainId key,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);

    Task RemoveAsync(DomainId key,
        CancellationToken ct = default);

    IAsyncEnumerable<SnapshotResult<T>> ReadAllAsync(
        CancellationToken ct = default);
}

public record struct SnapshotResult<T>(DomainId Key, T Value, long Version, bool IsValid = true);

public record struct SnapshotWriteJob<T>(DomainId Key, T Value, long NewVersion, long OldVersion = EtagVersion.Any)
{
    public readonly SnapshotWriteJob<TOther> As<TOther>(TOther snapshot)
    {
        return new SnapshotWriteJob<TOther>(Key, snapshot, NewVersion, OldVersion);
    }
}
