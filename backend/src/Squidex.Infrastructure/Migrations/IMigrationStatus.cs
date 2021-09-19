// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigrationStatus
    {
        Task<int> GetVersionAsync(
            CancellationToken ct = default);

        Task<bool> TryLockAsync(
            CancellationToken ct = default);

        Task CompleteAsync(int newVersion,
            CancellationToken ct = default);

        Task UnlockAsync(
            CancellationToken ct = default);
    }
}
