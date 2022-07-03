// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupGrain : IGrainWithStringKey
    {
        Task BackupAsync(RefToken actor);

        Task DeleteAsync(DomainId id);

        Task ClearAsync();

        Task<List<IBackupJob>> GetStateAsync();
    }
}
