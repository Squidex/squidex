// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupGrain : IGrainWithStringKey
    {
        Task BackupAsync(RefToken actor);

        Task DeleteAsync(DomainId id);

        Task ClearAsync();

        Task<J<List<IBackupJob>>> GetStateAsync();
    }
}
