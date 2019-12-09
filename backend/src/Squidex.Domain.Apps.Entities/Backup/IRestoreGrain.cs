// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IRestoreGrain : IGrainWithStringKey
    {
        Task RestoreAsync(Uri url, RefToken actor, string? newAppName = null);

        Task<J<IRestoreJob>> GetStateAsync();
    }
}
