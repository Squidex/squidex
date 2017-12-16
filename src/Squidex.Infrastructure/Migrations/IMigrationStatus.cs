// ==========================================================================
//  IMigrationState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigrationStatus
    {
        Task<int> GetVersionAsync();

        Task<bool> TryLockAsync(int currentVersion);

        Task UnlockAsync(int newVersion);
    }
}
