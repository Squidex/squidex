// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsIndex
    {
        Task<IAppEntity> GetAppAsync(string name);

        Task<IAppEntity> GetAppAsync(Guid appId);
    }
}
