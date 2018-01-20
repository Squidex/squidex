// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<Guid> FindAppIdByNameAsync(string name);

        Task<IReadOnlyList<Guid>> QueryAppIdsAsync();

        Task<IReadOnlyList<Guid>> QueryUserAppIdsAsync(string userId);
    }
}
