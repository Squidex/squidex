// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupGrain : IGrainWithGuidKey
    {
        Task RunAsync();

        Task DeleteAsync(Guid id);

        Task<J<List<IBackupJob>>> GetStateAsync();
    }
}
