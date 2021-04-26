// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigrationStatus
    {
        Task<int> GetVersionAsync();

        Task<bool> TryLockAsync();

        Task CompleteAsync(int newVersion);

        Task UnlockAsync();
    }
}
