// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigrationStatus
    {
        Task<int> GetVersionAsync();

        Task<bool> TryLockAsync();

        Task UnlockAsync(int newVersion);
    }
}
