// ==========================================================================
//  ISchemaRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Schemas.Repositories
{
    public interface ISchemaRepository
    {
        Task<IReadOnlyList<Guid>> QueryAllSchemaIdsAsync(Guid appId, string name);

        Task<IReadOnlyList<Guid>> QueryAllSchemaIdsAsync(Guid appId);
    }
}
